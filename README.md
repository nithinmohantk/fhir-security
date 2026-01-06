# FHIR Security Implementation Resources

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![FHIR R4](https://img.shields.io/badge/FHIR-R4-blue)](https://hl7.org/fhir/R4/)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![Python 3.9+](https://img.shields.io/badge/Python-3.9+-green)](https://python.org/)
[![HIPAA](https://img.shields.io/badge/Compliance-HIPAA-red)](https://www.hhs.gov/hipaa/)
[![GDPR](https://img.shields.io/badge/Compliance-GDPR-blue)](https://gdpr.eu/)

Production-ready code examples, compliance checklists, and monitoring templates for implementing FAPI 2.0, DPoP, and real-time FHIR pipelines in healthcare environments.

## 📋 Overview

This repository contains the resources referenced in the FHIR Security article series on [dataa.dev](https://www.dataa.dev):

1. [FHIR API Security Part 1: Foundation & Authentication](https://www.dataa.dev/2025/08/10/fhir-api-security-complete-guide-to-authentication-authorization-and-fapi-2-0/)
2. [FHIR API Security Part 2: Implementation & Best Practices](https://www.dataa.dev/2025/08/17/fhir-api-security-part-2-implementation-best-practices/)
3. [Real-Time Healthcare Data Pipelines: Kafka + FHIR](https://www.dataa.dev/2025/09/07/real-time-healthcare-data-pipelines-kafka-fhir-for-clinical-decision-support/)

## 📁 Repository Structure

```
fhir-security/
│
├── checklists/                     # Compliance & Security Checklists
│   ├── FHIR_Security_Deployment_Checklist.md   # 60+ security validation points
│   ├── GDPR_Compliance_Checklist.md            # GDPR to FHIR mapping (100+ points)
│   └── HIPAA_Compliance_Mapping.md             # HIPAA Security Rule mapping (80+ points)
│
├── dpop-examples/                  # DPoP Implementation (C#/.NET 8)
│   └── src/
│       ├── DPopClient.cs           # Production DPoP client implementation
│       └── FhirSecurityClient.cs   # FHIR client with DPoP authentication
│
├── kafka-examples/                 # Kafka + FHIR Event Streaming
│   ├── producer/
│   │   └── FhirKafkaProducer.cs    # .NET Kafka producer for FHIR events
│   └── consumer/
│       └── SepsisDetectionConsumer.py  # Python real-time qSOFA monitoring
│
├── monitoring/                     # Observability Templates
│   ├── grafana/
│   │   ├── fhir-api-dashboard.json      # API performance dashboard
│   │   └── security-alerts-dashboard.json  # Security monitoring dashboard
│   └── prometheus/
│       ├── prometheus.yml          # Prometheus scrape configuration
│       └── alert-rules.yml         # 20+ security alert rules
│
├── docker/                         # Local Development Environment
│   └── docker-compose.yml          # Full stack (FHIR, Kafka, Redis, Grafana)
│
├── LICENSE                         # MIT License + Healthcare disclaimer
└── README.md                       # This file
```

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- Python 3.9+
- Docker & Docker Compose
- Kafka (local or cloud)

### Local Development Environment

```bash
# Start the full stack (FHIR Server, Kafka, Redis, Prometheus, Grafana)
cd docker
docker-compose up -d

# Access:
# - FHIR Server: http://localhost:8080/fhir
# - Kafka UI: http://localhost:8081
# - Grafana: http://localhost:3000 (admin/admin)
# - Prometheus: http://localhost:9090
```

### DPoP Client Setup (.NET)

```bash
cd dpop-examples
dotnet restore
dotnet build
```

### Kafka Consumer Setup (Python)

```bash
cd kafka-examples/consumer
pip install confluent-kafka redis requests
python SepsisDetectionConsumer.py
```

## 📋 Compliance Checklists

| Checklist | Points | Description |
|-----------|--------|-------------|
| [FHIR Security Deployment](checklists/FHIR_Security_Deployment_Checklist.md) | 60+ | Production deployment validation |
| [GDPR Compliance](checklists/GDPR_Compliance_Checklist.md) | 100+ | EU data protection requirements |
| [HIPAA Compliance](checklists/HIPAA_Compliance_Mapping.md) | 80+ | US healthcare security requirements |

### Checklist Highlights

**FHIR Security Deployment:**
- Network security (TLS, WAF, DDoS)
- Authentication (OAuth 2.0, MFA, DPoP)
- Authorization (SMART scopes)
- Audit logging & monitoring
- Sign-off sections

**GDPR Compliance:**
- Data subject rights (Articles 15-22)
- FHIR Consent resource mapping
- Right to Erasure implementation
- Data portability ($export)
- International transfers

**HIPAA Security Rule:**
- Administrative safeguards (§164.308)
- Physical safeguards (§164.310)
- Technical safeguards (§164.312)
- AuditEvent resource examples
- BAA requirements

## 💻 Code Examples

| Example | Language | Lines | Description |
|---------|----------|-------|-------------|
| [DPopClient.cs](dpop-examples/src/DPopClient.cs) | C# | ~250 | RFC 9449 DPoP implementation |
| [FhirSecurityClient.cs](dpop-examples/src/FhirSecurityClient.cs) | C# | ~180 | FHIR client with DPoP auth |
| [FhirKafkaProducer.cs](kafka-examples/producer/FhirKafkaProducer.cs) | C# | ~250 | Event-driven FHIR publishing |
| [SepsisDetectionConsumer.py](kafka-examples/consumer/SepsisDetectionConsumer.py) | Python | ~300 | Real-time qSOFA sepsis detection |

### Key Features

- ✅ **DPoP Client:** ES256 key generation, nonce handling, token refresh
- ✅ **FHIR Client:** CRUD operations, $everything, Bulk Export
- ✅ **Kafka Producer:** SASL/SSL, Snappy compression, batch publishing
- ✅ **Sepsis Consumer:** qSOFA criteria, PagerDuty alerts, Redis state

## 📊 Monitoring Dashboards

| Dashboard | Panels | Metrics |
|-----------|--------|---------|
| [FHIR API Performance](monitoring/grafana/fhir-api-dashboard.json) | 12 | Latency, throughput, errors, DPoP |
| [Security Alerts](monitoring/grafana/security-alerts-dashboard.json) | 10 | Auth failures, replay attacks, anomalies |

### Prometheus Alert Rules

20+ pre-configured alerts including:
- `HighAuthenticationFailureRate` - >10 failures/sec
- `TokenReplayAttackDetected` - DPoP replay attempts
- `BulkExportAbuse` - Suspicious data exfiltration
- `KafkaConsumerLagCritical` - Clinical alert delays
- `AuditLogGap` - HIPAA compliance risk

## 🔐 Security Notes

**⚠️ Important:** Before using in production:

1. **Replace credentials** - No hardcoded secrets
2. **Configure certificates** - Use proper PKI/HSM
3. **Enable audit logging** - 6-year HIPAA retention
4. **Review compliance** - Complete checklists
5. **Penetration testing** - Conduct annual tests
6. **Legal review** - Get compliance sign-off

## 📄 License

MIT License - See [LICENSE](LICENSE) for details.

**Healthcare Disclaimer:** This software is provided for educational purposes. Always conduct your own security review, HIPAA compliance assessment, and legal review before production deployment.

## 🤝 Contributing

Contributions welcome! Areas of focus:
- Additional compliance frameworks (SOC 2, ISO 27001)
- More language examples (Java, Go)
- Enhanced monitoring dashboards
- Terraform/IaC templates

## 📬 Contact

- **Author:** Nithin Mohan T K
- **Blog:** [dataa.dev](https://www.dataa.dev)
- **LinkedIn:** [nithinmohantk](https://linkedin.com/in/nithinmohantk)
- **Repository:** [github.com/nithinmohantk/fhir-security](https://github.com/nithinmohantk/fhir-security)

---

## ⭐ Star History

If you find this repository useful, please consider giving it a star! It helps others discover these resources.

---

*Last updated: January 2026 | Total: 4,200+ lines of production-ready code and 240+ compliance validation points*
