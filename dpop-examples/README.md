# DPoP (Demonstrating Proof-of-Possession) Examples

Production-ready DPoP implementation for FHIR APIs with FAPI 2.0 security.

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- A FHIR server that supports OAuth 2.0/DPoP (e.g., HAPI FHIR with IdentityServer)

## Quick Start

### 1. Build the Project

```bash
cd dpop-examples
dotnet restore
dotnet build
```

### 2. Configure Environment Variables

```bash
# Windows PowerShell
$env:FHIR_SERVER_URL = "https://your-fhir-server.com/fhir"
$env:TOKEN_ENDPOINT = "https://your-auth-server.com/oauth2/token"
$env:CLIENT_ID = "your-client-id"

# Linux/macOS
export FHIR_SERVER_URL="https://your-fhir-server.com/fhir"
export TOKEN_ENDPOINT="https://your-auth-server.com/oauth2/token"
export CLIENT_ID="your-client-id"
```

### 3. Run the Example

```bash
dotnet run
```

## NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Hl7.Fhir.R4` | 5.5.1 | FHIR R4 resource models and serialization |
| `System.IdentityModel.Tokens.Jwt` | 7.2.0 | JWT creation and validation |
| `Microsoft.IdentityModel.Tokens` | 7.2.0 | Token handling and cryptography |
| `Microsoft.IdentityModel.JsonWebTokens` | 7.2.0 | JSON Web Token support |
| `System.Net.Http.Json` | 8.0.0 | HTTP JSON extensions |
| `System.Text.Json` | 8.0.4 | JSON serialization |
| `Microsoft.Extensions.Logging.Abstractions` | 8.0.1 | Logging interface |

## Project Structure

```
dpop-examples/
├── FhirSecurity.DPoP.csproj    # Project file with dependencies
├── README.md                    # This file
└── src/
    ├── DPopClient.cs            # Core DPoP client implementation
    ├── FhirSecurityClient.cs    # High-level FHIR client with DPoP
    └── Program.cs               # Example usage
```

## Key Files

### DPopClient.cs

Core DPoP implementation following [RFC 9449](https://datatracker.ietf.org/doc/html/rfc9449):

- ES256 key pair generation
- DPoP proof creation (jti, htm, htu, iat, ath)
- Nonce handling for replay protection
- Token refresh with DPoP binding

### FhirSecurityClient.cs

High-level FHIR client with DPoP authentication:

- CRUD operations (Read, Create, Update)
- Search with parameters
- $everything operation
- Automatic token refresh

### Program.cs

Example demonstrating:

- Direct DPoP client usage
- High-level FHIR client usage
- Patient search and retrieval

## Usage Examples

### Basic DPoP Client

```csharp
using FhirSecurity.DPoP;

using var client = new DPopClient(
    tokenEndpoint: "https://auth.example.com/oauth2/token",
    clientId: "my-client"
);

// Authenticate
await client.AuthenticateAsync("system/*.read");

// Make authenticated request
var response = await client.SendAsync(
    HttpMethod.Get, 
    "https://fhir.example.com/fhir/Patient/12345"
);
```

### FHIR Security Client

```csharp
using FhirSecurity.DPoP;
using Hl7.Fhir.Model;

using var fhirClient = new FhirSecurityClient(
    fhirBaseUrl: "https://fhir.example.com/fhir",
    tokenEndpoint: "https://auth.example.com/oauth2/token",
    clientId: "my-client"
);

// Authenticate
await fhirClient.AuthenticateAsync("system/*.read");

// Search patients
var bundle = await fhirClient.SearchAsync<Patient>(
    new Dictionary<string, string> { ["name"] = "Smith" }
);

// Read specific patient
var patient = await fhirClient.ReadAsync<Patient>("12345");

// Get all patient data
var everything = await fhirClient.PatientEverythingAsync("12345");
```

## Security Notes

⚠️ **Before using in production:**

1. **Never hardcode credentials** - Use environment variables or Key Vault
2. **Use proper certificate management** - Consider HSM for key storage
3. **Implement proper logging** - Replace the simple ILogger interface
4. **Add retry logic** - Handle transient failures
5. **Validate your auth server** - Ensure DPoP support is properly configured

## Testing with Local FHIR Server

Use the Docker Compose in the repository root:

```bash
cd ..
docker-compose -f docker/docker-compose.yml up -d
```

This starts:
- HAPI FHIR Server (http://localhost:8080)
- Keycloak for OAuth (http://localhost:8443)

## Related Articles

- [FHIR API Security Part 1: Foundation & Authentication](https://www.dataa.dev/2025/08/10/fhir-api-security-complete-guide-to-authentication-authorization-and-fapi-2-0/)
- [FHIR API Security Part 2: Implementation & Best Practices](https://www.dataa.dev/2025/08/17/fhir-api-security-part-2-implementation-best-practices/)

## License

MIT License - See [LICENSE](../LICENSE) for details.
