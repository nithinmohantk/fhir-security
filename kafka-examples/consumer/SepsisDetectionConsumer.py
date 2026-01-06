"""
Sepsis Detection Consumer for FHIR Kafka Pipeline

Real-time qSOFA (quick Sequential Organ Failure Assessment) monitoring
using Kafka + FHIR for clinical decision support.

Author: Nithin Mohan T K
Repository: https://github.com/nithinmohantk/fhir-security

Usage:
    python SepsisDetectionConsumer.py

Environment Variables:
    KAFKA_BOOTSTRAP_SERVERS: Kafka broker addresses
    KAFKA_USERNAME: SASL username (optional)
    KAFKA_PASSWORD: SASL password (optional)
    REDIS_HOST: Redis host for state management
    PAGERDUTY_API_KEY: PagerDuty integration key (optional)
"""

import os
import json
import logging
from datetime import datetime, timedelta
from typing import Dict, Optional, Any

from confluent_kafka import Consumer, KafkaError, KafkaException
import redis

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger('SepsisDetection')


class SepsisDetectionConsumer:
    """
    Real-time sepsis detection consumer using qSOFA criteria.
    
    qSOFA Criteria (2 or more = positive screen):
    - Respiratory rate >= 22/min
    - Altered mentation (GCS < 15)
    - Systolic BP <= 100 mmHg
    """
    
    # LOINC codes for vital signs
    LOINC_CODES = {
        '9279-1': 'respiratory_rate',
        '85354-9': 'systolic_bp',
        '9269-2': 'gcs',
        '8867-4': 'heart_rate',
        '8310-5': 'temperature'
    }
    
    # qSOFA thresholds
    THRESHOLDS = {
        'respiratory_rate': 22,  # >= 22 is abnormal
        'systolic_bp': 100,      # <= 100 is abnormal
        'gcs': 15                # < 15 is abnormal
    }
    
    def __init__(
        self,
        bootstrap_servers: str,
        group_id: str = 'sepsis-detection-consumer',
        redis_host: str = 'localhost',
        redis_port: int = 6379
    ):
        """Initialize the sepsis detection consumer."""
        
        # Kafka configuration
        kafka_config = {
            'bootstrap.servers': bootstrap_servers,
            'group.id': group_id,
            'auto.offset.reset': 'earliest',
            'enable.auto.commit': False,  # Manual commit for reliability
            'max.poll.interval.ms': 300000,  # 5 minutes
            'session.timeout.ms': 60000,     # 60 seconds
        }
        
        # Add SASL authentication if credentials provided
        username = os.getenv('KAFKA_USERNAME')
        password = os.getenv('KAFKA_PASSWORD')
        
        if username and password:
            kafka_config.update({
                'security.protocol': 'SASL_SSL',
                'sasl.mechanism': 'PLAIN',
                'sasl.username': username,
                'sasl.password': password
            })
            logger.info("Kafka configured with SASL_SSL authentication")
        else:
            logger.warning("Kafka running without authentication (not recommended for production)")
        
        self.consumer = Consumer(kafka_config)
        
        # Subscribe to vital signs and lab topics
        self.consumer.subscribe([
            'fhir.observation.vitals',
            'fhir.observation.labs'
        ])
        
        # Redis for patient state tracking
        self.redis = redis.Redis(
            host=redis_host,
            port=redis_port,
            decode_responses=True
        )
        
        # Alert cooldown (prevent spam)
        self.alert_cooldown_minutes = 60
        
        logger.info(f"Sepsis detection consumer initialized. Group: {group_id}")
    
    def process_messages(self):
        """Main message processing loop."""
        logger.info("Starting message processing...")
        
        try:
            while True:
                msg = self.consumer.poll(timeout=1.0)
                
                if msg is None:
                    continue
                
                if msg.error():
                    if msg.error().code() == KafkaError._PARTITION_EOF:
                        # End of partition, not an error
                        continue
                    else:
                        logger.error(f"Consumer error: {msg.error()}")
                        raise KafkaException(msg.error())
                
                # Process the message
                try:
                    self._process_message(msg)
                    # Commit after successful processing
                    self.consumer.commit(asynchronous=False)
                except Exception as e:
                    logger.error(f"Error processing message: {e}", exc_info=True)
                    # Don't commit - message will be reprocessed
                    
        except KeyboardInterrupt:
            logger.info("Shutdown requested...")
        finally:
            self.consumer.close()
            logger.info("Consumer closed")
    
    def _process_message(self, msg):
        """Process a single Kafka message."""
        # Parse event envelope
        event = json.loads(msg.value().decode('utf-8'))
        patient_id = event.get('PatientId', 'unknown')
        resource_json = event.get('ResourceJson', '{}')
        
        # Parse FHIR Observation
        observation = json.loads(resource_json)
        
        # Check if this is a relevant observation
        if observation.get('resourceType') != 'Observation':
            return
        
        # Extract code and value
        coding = observation.get('code', {}).get('coding', [{}])[0]
        code = coding.get('code')
        
        # Get value (handle both valueQuantity and valueInteger)
        value = None
        if 'valueQuantity' in observation:
            value = observation['valueQuantity'].get('value')
        elif 'valueInteger' in observation:
            value = observation['valueInteger']
        
        if code not in self.LOINC_CODES or value is None:
            return
        
        vital_sign = self.LOINC_CODES[code]
        
        logger.debug(f"Patient {patient_id}: {vital_sign} = {value}")
        
        # Check and update qSOFA criteria
        if self._check_qsofa_criteria(patient_id, vital_sign, value):
            self._trigger_sepsis_alert(patient_id, observation)
    
    def _check_qsofa_criteria(
        self, 
        patient_id: str, 
        vital_sign: str, 
        value: float
    ) -> bool:
        """
        Check if patient meets qSOFA criteria.
        Returns True if 2+ criteria are met.
        """
        criteria_key = f"sepsis:criteria:{patient_id}"
        
        # Check if this vital sign is abnormal
        is_abnormal = False
        threshold = self.THRESHOLDS.get(vital_sign)
        
        if threshold:
            if vital_sign == 'respiratory_rate':
                is_abnormal = value >= threshold
            elif vital_sign == 'systolic_bp':
                is_abnormal = value <= threshold
            elif vital_sign == 'gcs':
                is_abnormal = value < threshold
        
        if is_abnormal:
            # Store abnormal finding (expires after 6 hours)
            self.redis.hset(criteria_key, vital_sign, str(value))
            self.redis.expire(criteria_key, 21600)  # 6 hours
            logger.info(f"Patient {patient_id}: Abnormal {vital_sign}={value}")
        else:
            # Clear this criterion if now normal
            self.redis.hdel(criteria_key, vital_sign)
        
        # Count how many criteria are met
        criteria_met = self.redis.hgetall(criteria_key)
        qsofa_score = len(criteria_met)
        
        logger.debug(f"Patient {patient_id}: qSOFA score = {qsofa_score}")
        
        return qsofa_score >= 2
    
    def _trigger_sepsis_alert(self, patient_id: str, observation: dict):
        """
        Trigger sepsis alert to rapid response team.
        """
        # Check cooldown to prevent alert spam
        alert_key = f"sepsis:alert:{patient_id}"
        if self.redis.exists(alert_key):
            logger.debug(f"Patient {patient_id}: Alert already sent (cooldown active)")
            return
        
        # Get current criteria
        criteria_key = f"sepsis:criteria:{patient_id}"
        criteria = self.redis.hgetall(criteria_key)
        
        # Build alert
        alert = {
            'alert_type': 'SEPSIS_RISK',
            'severity': 'HIGH',
            'patient_id': patient_id,
            'timestamp': datetime.utcnow().isoformat(),
            'qsofa_score': len(criteria),
            'criteria_met': criteria,
            'triggering_observation': observation.get('id'),
            'message': f'Patient {patient_id} meets qSOFA sepsis screening criteria ({len(criteria)}/3 positive)'
        }
        
        # Send alerts
        self._send_to_pagerduty(alert)
        self._send_to_ehr(alert)
        self._log_to_audit(alert)
        
        # Set alert cooldown
        self.redis.setex(alert_key, self.alert_cooldown_minutes * 60, '1')
        
        logger.warning(f"🚨 SEPSIS ALERT: Patient {patient_id} - qSOFA {len(criteria)}/3")
    
    def _send_to_pagerduty(self, alert: dict):
        """Send high-priority page to rapid response team."""
        api_key = os.getenv('PAGERDUTY_API_KEY')
        if not api_key:
            logger.debug("PagerDuty not configured, skipping")
            return
        
        # PagerDuty Events API v2
        import requests
        
        payload = {
            'routing_key': api_key,
            'event_action': 'trigger',
            'dedup_key': f"sepsis-{alert['patient_id']}",
            'payload': {
                'summary': alert['message'],
                'severity': 'critical',
                'source': 'FHIR-Sepsis-Detection',
                'custom_details': alert
            }
        }
        
        try:
            response = requests.post(
                'https://events.pagerduty.com/v2/enqueue',
                json=payload,
                timeout=5
            )
            if response.ok:
                logger.info(f"PagerDuty alert sent for patient {alert['patient_id']}")
            else:
                logger.error(f"PagerDuty error: {response.text}")
        except Exception as e:
            logger.error(f"Failed to send PagerDuty alert: {e}")
    
    def _send_to_ehr(self, alert: dict):
        """Create in-basket alert in EHR system."""
        # This would integrate with Epic/Cerner/etc. via their API
        # Placeholder for EHR integration
        logger.info(f"EHR alert would be sent for patient {alert['patient_id']}")
    
    def _log_to_audit(self, alert: dict):
        """Log alert to HIPAA audit trail."""
        # SIEM integration or audit log
        audit_entry = {
            'event_type': 'CLINICAL_ALERT',
            'alert': alert,
            'timestamp': datetime.utcnow().isoformat(),
            'system': 'sepsis-detection'
        }
        logger.info(f"AUDIT: {json.dumps(audit_entry)}")


def main():
    """Entry point for the sepsis detection consumer."""
    bootstrap_servers = os.getenv('KAFKA_BOOTSTRAP_SERVERS', 'localhost:9092')
    redis_host = os.getenv('REDIS_HOST', 'localhost')
    
    consumer = SepsisDetectionConsumer(
        bootstrap_servers=bootstrap_servers,
        redis_host=redis_host
    )
    
    consumer.process_messages()


if __name__ == '__main__':
    main()
