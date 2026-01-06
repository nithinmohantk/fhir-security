# Kafka + FHIR Examples

Real-time healthcare data pipelines with Apache Kafka and FHIR.

## Structure

```
kafka-examples/
├── producer/                    # .NET Kafka Producer
│   ├── FhirSecurity.Kafka.csproj
│   ├── FhirKafkaProducer.cs
│   └── README.md
│
└── consumer/                    # Python Kafka Consumer
    ├── SepsisDetectionConsumer.py
    ├── requirements.txt
    └── README.md
```

## Producer (.NET)

Production-ready Kafka producer for FHIR events.

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Apache Kafka cluster (or Confluent Cloud)

### Build & Run

```bash
cd producer
dotnet restore
dotnet build
```

### NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Hl7.Fhir.R4` | 5.5.1 | FHIR R4 resources |
| `Confluent.Kafka` | 2.3.0 | Kafka client |
| `System.Text.Json` | 8.0.4 | JSON serialization |

### Usage

```csharp
using FhirSecurity.Kafka;
using Hl7.Fhir.Model;

var producer = new FhirKafkaProducer(
    bootstrapServers: Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS"),
    username: Environment.GetEnvironmentVariable("KAFKA_USERNAME"),
    password: Environment.GetEnvironmentVariable("KAFKA_PASSWORD")
);

// Publish a vital sign observation
var observation = new Observation
{
    Id = "obs-123",
    Status = ObservationStatus.Final,
    Code = new CodeableConcept("http://loinc.org", "8867-4", "Heart rate"),
    Subject = new ResourceReference("Patient/12345"),
    Value = new Quantity(72, "beats/minute")
};

await producer.PublishObservationAsync(observation);
```

---

## Consumer (Python)

Real-time qSOFA sepsis detection using FHIR vital signs.

### Prerequisites

- Python 3.9+
- Apache Kafka cluster
- Redis (for state management)

### Install Dependencies

```bash
cd consumer
pip install -r requirements.txt
```

### Python Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `confluent-kafka` | ≥2.3.0 | Kafka consumer |
| `redis` | ≥5.0.0 | Patient state storage |
| `requests` | ≥2.31.0 | PagerDuty/EHR integration |
| `fhir.resources` | ≥7.0.0 | FHIR parsing (optional) |
| `python-dotenv` | ≥1.0.0 | Environment variables |
| `prometheus-client` | ≥0.19.0 | Metrics (optional) |

### Run

```bash
# Set environment variables
export KAFKA_BOOTSTRAP_SERVERS="localhost:9092"
export REDIS_HOST="localhost"
export PAGERDUTY_API_KEY="your-api-key"  # Optional

# Run consumer
python SepsisDetectionConsumer.py
```

### How It Works

The consumer implements qSOFA (quick Sequential Organ Failure Assessment):

| Criterion | LOINC Code | Threshold |
|-----------|------------|-----------|
| Respiratory Rate | 9279-1 | ≥ 22/min |
| Systolic Blood Pressure | 85354-9 | ≤ 100 mmHg |
| Glasgow Coma Scale | 9269-2 | < 15 |

**Sepsis Alert Triggered:** When 2+ criteria are met within 6 hours.

---

## Local Development

### Start Local Stack

```bash
cd ../docker
docker-compose up -d
```

This starts:
- Kafka on `localhost:9092`
- Redis on `localhost:6379`
- FHIR Server on `localhost:8080`

### Kafka Topics

The producer automatically creates these topics:

| Topic | Content |
|-------|---------|
| `fhir.observation.vitals` | Vital sign observations |
| `fhir.observation.labs` | Laboratory results |
| `fhir.encounter` | Encounter events |
| `fhir.medicationrequest` | Medication orders |

---

## Security

⚠️ **HIPAA Compliance:**

- All Kafka connections use SASL/SSL (when credentials provided)
- Data at rest should be encrypted
- Consumer commits are manual for exactly-once processing
- All access is logged for audit

---

## Related Articles

- [Real-Time Healthcare Data Pipelines: Kafka + FHIR](https://www.dataa.dev/2025/09/07/real-time-healthcare-data-pipelines-kafka-fhir-for-clinical-decision-support/)

## License

MIT License - See [LICENSE](../LICENSE) for details.
