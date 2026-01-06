using System.Net.Http.Json;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirSecurity.DPoP;

/// <summary>
/// FHIR client with DPoP authentication support.
/// Implements SMART on FHIR authorization with FAPI 2.0 security.
/// 
/// Author: Nithin Mohan T K
/// Repository: https://github.com/nithinmohantk/fhir-security
/// </summary>
public class FhirSecurityClient : IDisposable
{
    private readonly DPopClient _dpopClient;
    private readonly string _fhirBaseUrl;
    private readonly FhirJsonSerializer _serializer;
    private readonly FhirJsonParser _parser;
    private readonly ILogger<FhirSecurityClient>? _logger;

    public FhirSecurityClient(
        string fhirBaseUrl,
        string tokenEndpoint,
        string clientId,
        ILogger<FhirSecurityClient>? logger = null)
    {
        _fhirBaseUrl = fhirBaseUrl.TrimEnd('/');
        _logger = logger;
        
        // Initialize DPoP client
        _dpopClient = new DPopClient(
            tokenEndpoint, 
            clientId, 
            logger != null ? new DPopLoggerAdapter<DPopClient>(logger) : null);
        
        _serializer = new FhirJsonSerializer(new SerializerSettings { Pretty = false });
        _parser = new FhirJsonParser();
    }

    /// <summary>
    /// Authenticates with the FHIR server using DPoP-bound tokens.
    /// </summary>
    public async Task AuthenticateAsync(string scope, CancellationToken ct = default)
    {
        _logger?.LogInformation("Authenticating with FHIR server at {BaseUrl}", _fhirBaseUrl);
        await _dpopClient.AuthenticateAsync(scope, ct);
        _logger?.LogInformation("Authentication successful");
    }

    /// <summary>
    /// Reads a FHIR resource by type and ID.
    /// </summary>
    public async Task<T?> ReadAsync<T>(string id, CancellationToken ct = default) where T : Resource
    {
        var resourceType = ModelInfo.GetFhirTypeNameForType(typeof(T));
        var url = $"{_fhirBaseUrl}/{resourceType}/{id}";
        
        _logger?.LogDebug("Reading {ResourceType}/{Id}", resourceType, id);
        
        var response = await _dpopClient.SendAsync(HttpMethod.Get, url, ct: ct);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogWarning("Read failed: {StatusCode}", response.StatusCode);
            return null;
        }
        
        var json = await response.Content.ReadAsStringAsync(ct);
        return _parser.Parse<T>(json);
    }

    /// <summary>
    /// Searches for FHIR resources with query parameters.
    /// </summary>
    public async Task<Bundle?> SearchAsync<T>(
        Dictionary<string, string>? parameters = null, 
        CancellationToken ct = default) where T : Resource
    {
        var resourceType = ModelInfo.GetFhirTypeNameForType(typeof(T));
        var queryString = parameters != null 
            ? "?" + string.Join("&", parameters.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"))
            : "";
        
        var url = $"{_fhirBaseUrl}/{resourceType}{queryString}";
        
        _logger?.LogDebug("Searching {ResourceType} with {ParameterCount} parameters", 
            resourceType, parameters?.Count ?? 0);
        
        var response = await _dpopClient.SendAsync(HttpMethod.Get, url, ct: ct);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(ct);
        return _parser.Parse<Bundle>(json);
    }

    /// <summary>
    /// Creates a new FHIR resource.
    /// </summary>
    public async Task<T?> CreateAsync<T>(T resource, CancellationToken ct = default) where T : Resource
    {
        var resourceType = ModelInfo.GetFhirTypeNameForType(typeof(T));
        var url = $"{_fhirBaseUrl}/{resourceType}";
        
        var json = _serializer.SerializeToString(resource);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/fhir+json");
        
        _logger?.LogInformation("Creating {ResourceType}", resourceType);
        
        var response = await _dpopClient.SendAsync(HttpMethod.Post, url, content, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogError(null!, "Create failed: {StatusCode}", response.StatusCode);
            return null;
        }
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        return _parser.Parse<T>(responseJson);
    }

    /// <summary>
    /// Updates an existing FHIR resource.
    /// </summary>
    public async Task<T?> UpdateAsync<T>(T resource, CancellationToken ct = default) where T : Resource
    {
        if (string.IsNullOrEmpty(resource.Id))
            throw new ArgumentException("Resource must have an ID for update");
        
        var resourceType = ModelInfo.GetFhirTypeNameForType(typeof(T));
        var url = $"{_fhirBaseUrl}/{resourceType}/{resource.Id}";
        
        var json = _serializer.SerializeToString(resource);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/fhir+json");
        
        _logger?.LogInformation("Updating {ResourceType}/{Id}", resourceType, resource.Id);
        
        var response = await _dpopClient.SendAsync(HttpMethod.Put, url, content, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger?.LogError(null!, "Update failed: {StatusCode}", response.StatusCode);
            return null;
        }
        
        var responseJson = await response.Content.ReadAsStringAsync(ct);
        return _parser.Parse<T>(responseJson);
    }

    /// <summary>
    /// Executes a FHIR $everything operation for a patient.
    /// Requires appropriate scopes (patient/*.$everything or system/*.$everything)
    /// </summary>
    public async Task<Bundle?> PatientEverythingAsync(
        string patientId, 
        CancellationToken ct = default)
    {
        var url = $"{_fhirBaseUrl}/Patient/{patientId}/$everything";
        
        _logger?.LogInformation("Executing $everything for Patient/{Id}", patientId);
        
        var response = await _dpopClient.SendAsync(HttpMethod.Get, url, ct: ct);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync(ct);
        return _parser.Parse<Bundle>(json);
    }

    public void Dispose()
    {
        _dpopClient?.Dispose();
    }
}

/// <summary>
/// Logger adapter to bridge generic logger interfaces.
/// </summary>
internal class DPopLoggerAdapter<T> : FhirSecurity.DPoP.ILogger<T>
{
    private readonly ILogger<FhirSecurityClient> _logger;
    
    public DPopLoggerAdapter(ILogger<FhirSecurityClient> logger) => _logger = logger;
    
    public void LogInformation(string message, params object[] args) => 
        _logger.LogInformation(message, args);
    public void LogDebug(string message, params object[] args) => 
        _logger.LogDebug(message, args);
    public void LogWarning(string message, params object[] args) => 
        _logger.LogWarning(message, args);
    public void LogError(Exception ex, string message, params object[] args) => 
        _logger.LogError(ex, message, args);
}
