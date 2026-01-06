using Confluent.Kafka;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text;
using System.Text.Json;

namespace FhirSecurity.Kafka;

/// <summary>
/// Production-ready Kafka producer for FHIR events.
/// Implements secure event streaming for healthcare data pipelines.
/// 
/// Author: Nithin Mohan T K
/// Repository: https://github.com/nithinmohantk/fhir-security
/// </summary>
public class FhirKafkaProducer : IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly FhirJsonSerializer _serializer;
    private readonly ILogger<FhirKafkaProducer>? _logger;

    public FhirKafkaProducer(
        string bootstrapServers,
        string? username = null,
        string? password = null,
        ILogger<FhirKafkaProducer>? logger = null)
    {
        _logger = logger;
        
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            
            // Reliability - Critical for healthcare
            Acks = Acks.All,              // Wait for all replicas
            EnableIdempotence = true,      // Exactly-once semantics
            MaxInFlight = 5,               // Max concurrent requests
            MessageSendMaxRetries = 3,     // Retry on failure
            RetryBackoffMs = 100,          // Backoff between retries
            
            // Performance
            CompressionType = CompressionType.Snappy,
            LingerMs = 10,                 // Batch for 10ms
            BatchSize = 16384,             // 16KB batches
            
            // Security - Required for healthcare (HIPAA §164.312)
            SecurityProtocol = string.IsNullOrEmpty(username) 
                ? SecurityProtocol.Plaintext 
                : SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = username ?? Environment.GetEnvironmentVariable("KAFKA_USERNAME"),
            SaslPassword = password ?? Environment.GetEnvironmentVariable("KAFKA_PASSWORD"),
            SslCaLocation = Environment.GetEnvironmentVariable("KAFKA_SSL_CA_LOCATION")
        };
        
        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => 
                _logger?.LogError("Kafka error: {Reason} (Code: {Code})", e.Reason, e.Code))
            .SetStatisticsHandler((_, json) => 
                _logger?.LogDebug("Kafka stats: {Stats}", json))
            .Build();
            
        _serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = false });
        
        _logger?.LogInformation("FHIR Kafka producer initialized");
    }

    /// <summary>
    /// Publishes a FHIR Observation to the appropriate topic.
    /// </summary>
    public async Task<DeliveryResult<string, string>> PublishObservationAsync(
        Observation observation,
        string eventType = "created",
        CancellationToken ct = default)
    {
        var topic = GetTopicForObservation(observation);
        return await PublishResourceAsync(observation, topic, eventType, ct);
    }

    /// <summary>
    /// Publishes a FHIR Encounter to Kafka.
    /// </summary>
    public async Task<DeliveryResult<string, string>> PublishEncounterAsync(
        Encounter encounter,
        string eventType = "created",
        CancellationToken ct = default)
    {
        return await PublishResourceAsync(encounter, "fhir.encounter", eventType, ct);
    }

    /// <summary>
    /// Publishes a FHIR MedicationRequest to Kafka.
    /// </summary>
    public async Task<DeliveryResult<string, string>> PublishMedicationRequestAsync(
        MedicationRequest medicationRequest,
        string eventType = "created",
        CancellationToken ct = default)
    {
        return await PublishResourceAsync(medicationRequest, "fhir.medicationrequest", eventType, ct);
    }

    /// <summary>
    /// Generic method to publish any FHIR resource.
    /// </summary>
    public async Task<DeliveryResult<string, string>> PublishResourceAsync<T>(
        T resource,
        string topic,
        string eventType = "created",
        CancellationToken ct = default) where T : Resource
    {
        try
        {
            var patientId = ExtractPatientId(resource);
            var resourceType = resource.TypeName;
            
            // Create event envelope with metadata
            var fhirEvent = new FhirEvent
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = eventType,
                EventTime = DateTimeOffset.UtcNow,
                ResourceType = resourceType,
                ResourceId = resource.Id ?? Guid.NewGuid().ToString(),
                PatientId = patientId,
                ResourceJson = _serializer.SerializeToString(resource)
            };
            
            // Use patient ID as partition key for ordering
            string key = patientId;
            string value = JsonSerializer.Serialize(fhirEvent);
            
            // Build message with headers for filtering
            var message = new Message<string, string>
            {
                Key = key,
                Value = value,
                Headers = new Headers
                {
                    { "event-type", Encoding.UTF8.GetBytes(eventType) },
                    { "resource-type", Encoding.UTF8.GetBytes(resourceType) },
                    { "event-id", Encoding.UTF8.GetBytes(fhirEvent.EventId) },
                    { "correlation-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()) },
                    { "source-system", Encoding.UTF8.GetBytes("FHIR-Server") },
                    { "timestamp", Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O")) }
                }
            };
            
            _logger?.LogDebug(
                "Publishing {EventType} event for {ResourceType}/{ResourceId} to {Topic}",
                eventType, resourceType, resource.Id, topic);
            
            var result = await _producer.ProduceAsync(topic, message, ct);
            
            _logger?.LogInformation(
                "Published {ResourceType}/{ResourceId} to {Topic}:{Partition}@{Offset}",
                resourceType, resource.Id, topic, result.Partition.Value, result.Offset.Value);
            
            return result;
        }
        catch (ProduceException<string, string> ex)
        {
            _logger?.LogError(ex, "Failed to publish {ResourceType} to Kafka", resource.TypeName);
            throw;
        }
    }

    /// <summary>
    /// Bulk publishes multiple resources efficiently.
    /// </summary>
    public async Task<int> PublishBatchAsync<T>(
        IEnumerable<T> resources,
        string topic,
        string eventType = "created",
        CancellationToken ct = default) where T : Resource
    {
        int count = 0;
        var tasks = new List<Task<DeliveryResult<string, string>>>();
        
        foreach (var resource in resources)
        {
            tasks.Add(PublishResourceAsync(resource, topic, eventType, ct));
            count++;
            
            // Batch in groups of 100
            if (tasks.Count >= 100)
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
        }
        
        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
        }
        
        _logger?.LogInformation("Published batch of {Count} resources to {Topic}", count, topic);
        return count;
    }

    private string GetTopicForObservation(Observation obs)
    {
        var category = obs.Category?.FirstOrDefault()
            ?.Coding?.FirstOrDefault()?.Code;
        
        return category switch
        {
            "vital-signs" => "fhir.observation.vitals",
            "laboratory" => "fhir.observation.labs",
            "imaging" => "fhir.observation.imaging",
            "survey" => "fhir.observation.survey",
            "procedure" => "fhir.observation.procedure",
            _ => "fhir.observation.other"
        };
    }

    private string ExtractPatientId<T>(T resource) where T : Resource
    {
        // Try to extract patient reference from common patterns
        var patientRef = resource switch
        {
            Observation obs => obs.Subject?.Reference,
            Encounter enc => enc.Subject?.Reference,
            MedicationRequest med => med.Subject?.Reference,
            Condition cond => cond.Subject?.Reference,
            Procedure proc => proc.Subject?.Reference,
            _ => null
        };
        
        if (!string.IsNullOrEmpty(patientRef))
        {
            return patientRef.Split('/').Last();
        }
        
        return "unknown";
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}

/// <summary>
/// Event envelope for FHIR events in Kafka.
/// </summary>
public class FhirEvent
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTimeOffset EventTime { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string PatientId { get; set; } = string.Empty;
    public string ResourceJson { get; set; } = string.Empty;
}

/// <summary>
/// Logger interface for the producer.
/// </summary>
public interface ILogger<T>
{
    void LogInformation(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
}
