using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace FhirSecurity.DPoP;

/// <summary>
/// Production-ready DPoP (Demonstrating Proof-of-Possession) client for FHIR APIs.
/// Implements RFC 9449 for sender-constrained tokens.
/// 
/// Author: Nithin Mohan T K
/// Repository: https://github.com/nithinmohantk/fhir-security
/// </summary>
public class DPopClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ECDsa _privateKey;
    private readonly string _publicKeyJwk;
    private readonly ILogger<DPopClient>? _logger;
    private readonly string _tokenEndpoint;
    private readonly string _clientId;
    private string? _accessToken;
    private string? _refreshToken;
    private DateTime _tokenExpiry;
    private string? _lastNonce;

    /// <summary>
    /// Initializes a new DPoP client with generated ephemeral keys.
    /// </summary>
    /// <param name="tokenEndpoint">OAuth token endpoint URL</param>
    /// <param name="clientId">OAuth client ID</param>
    /// <param name="logger">Optional logger</param>
    public DPopClient(string tokenEndpoint, string clientId, ILogger<DPopClient>? logger = null)
    {
        _tokenEndpoint = tokenEndpoint;
        _clientId = clientId;
        _logger = logger;
        
        // Generate ephemeral ES256 key pair
        _privateKey = ECDsa.Create(ECCurve.NamedCurves.nist256);
        _publicKeyJwk = ExportPublicKeyJwk(_privateKey);
        
        _httpClient = new HttpClient();
        
        _logger?.LogInformation("DPoP client initialized with ephemeral ES256 key");
    }

    /// <summary>
    /// Authenticates using client credentials with DPoP proof.
    /// </summary>
    public async Task<string> AuthenticateAsync(string scope, CancellationToken ct = default)
    {
        var dpopProof = CreateDPopProof("POST", _tokenEndpoint, _lastNonce);
        
        var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint);
        request.Headers.Add("DPoP", dpopProof);
        
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _clientId,
            ["scope"] = scope
        });

        var response = await _httpClient.SendAsync(request, ct);
        
        // Handle DPoP nonce requirement (RFC 9449 Section 5)
        if (response.Headers.TryGetValues("DPoP-Nonce", out var nonceValues))
        {
            _lastNonce = nonceValues.First();
            _logger?.LogDebug("Received DPoP nonce from server: {Nonce}", _lastNonce);
            
            // Retry with nonce if we got use_dpop_nonce error
            if (!response.IsSuccessStatusCode)
            {
                return await AuthenticateAsync(scope, ct);
            }
        }

        response.EnsureSuccessStatusCode();
        
        var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(
            await response.Content.ReadAsStreamAsync(ct), 
            cancellationToken: ct);

        _accessToken = tokenResponse!.AccessToken;
        _refreshToken = tokenResponse.RefreshToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30); // 30s buffer
        
        _logger?.LogInformation("Successfully obtained DPoP-bound access token. Expires: {Expiry}", _tokenExpiry);
        
        return _accessToken;
    }

    /// <summary>
    /// Makes an authenticated request to a FHIR API with DPoP proof.
    /// </summary>
    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, 
        string url, 
        HttpContent? content = null,
        CancellationToken ct = default)
    {
        // Refresh token if expired
        if (DateTime.UtcNow >= _tokenExpiry && _refreshToken != null)
        {
            await RefreshTokenAsync(ct);
        }

        if (string.IsNullOrEmpty(_accessToken))
        {
            throw new InvalidOperationException("Not authenticated. Call AuthenticateAsync first.");
        }

        var dpopProof = CreateDPopProof(method.Method, url, _lastNonce, _accessToken);
        
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("DPoP", _accessToken);
        request.Headers.Add("DPoP", dpopProof);
        
        if (content != null)
        {
            request.Content = content;
        }

        var response = await _httpClient.SendAsync(request, ct);
        
        // Handle nonce updates
        if (response.Headers.TryGetValues("DPoP-Nonce", out var nonceValues))
        {
            _lastNonce = nonceValues.First();
        }

        return response;
    }

    /// <summary>
    /// Creates a DPoP proof JWT per RFC 9449.
    /// </summary>
    private string CreateDPopProof(string httpMethod, string httpUri, string? nonce = null, string? accessToken = null)
    {
        var header = new Dictionary<string, object>
        {
            ["alg"] = "ES256",
            ["typ"] = "dpop+jwt",
            ["jwk"] = JsonSerializer.Deserialize<Dictionary<string, object>>(_publicKeyJwk)!
        };

        var payload = new Dictionary<string, object>
        {
            ["jti"] = Guid.NewGuid().ToString(),
            ["htm"] = httpMethod.ToUpperInvariant(),
            ["htu"] = httpUri,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        // Add nonce if server provided one
        if (!string.IsNullOrEmpty(nonce))
        {
            payload["nonce"] = nonce;
        }

        // Add access token hash if binding to existing token (ath claim)
        if (!string.IsNullOrEmpty(accessToken))
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
            payload["ath"] = Base64UrlEncoder.Encode(hash);
        }

        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);
        
        var headerB64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(headerJson));
        var payloadB64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payloadJson));
        
        var signingInput = $"{headerB64}.{payloadB64}";
        var signature = _privateKey.SignData(
            Encoding.UTF8.GetBytes(signingInput), 
            HashAlgorithmName.SHA256);
        var signatureB64 = Base64UrlEncoder.Encode(signature);
        
        return $"{headerB64}.{payloadB64}.{signatureB64}";
    }

    private async Task RefreshTokenAsync(CancellationToken ct)
    {
        var dpopProof = CreateDPopProof("POST", _tokenEndpoint, _lastNonce);
        
        var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint);
        request.Headers.Add("DPoP", dpopProof);
        
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = _clientId,
            ["refresh_token"] = _refreshToken!
        });

        var response = await _httpClient.SendAsync(request, ct);
        
        if (response.Headers.TryGetValues("DPoP-Nonce", out var nonceValues))
        {
            _lastNonce = nonceValues.First();
        }

        response.EnsureSuccessStatusCode();
        
        var tokenResponse = await JsonSerializer.DeserializeAsync<TokenResponse>(
            await response.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);

        _accessToken = tokenResponse!.AccessToken;
        _refreshToken = tokenResponse.RefreshToken ?? _refreshToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 30);
        
        _logger?.LogInformation("Successfully refreshed DPoP-bound access token");
    }

    private static string ExportPublicKeyJwk(ECDsa key)
    {
        var parameters = key.ExportParameters(false);
        
        var jwk = new Dictionary<string, string>
        {
            ["kty"] = "EC",
            ["crv"] = "P-256",
            ["x"] = Base64UrlEncoder.Encode(parameters.Q.X!),
            ["y"] = Base64UrlEncoder.Encode(parameters.Q.Y!)
        };
        
        return JsonSerializer.Serialize(jwk);
    }

    public void Dispose()
    {
        _privateKey?.Dispose();
        _httpClient?.Dispose();
    }
}

/// <summary>
/// OAuth token response model.
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
    
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

/// <summary>
/// Simple logger interface for DPoP client.
/// </summary>
public interface ILogger<T>
{
    void LogInformation(string message, params object[] args);
    void LogDebug(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
}
