using FhirSecurity.DPoP;
using Hl7.Fhir.Model;

namespace FhirSecurity.Examples;

/// <summary>
/// Example usage of the DPoP client and FHIR Security Client.
/// 
/// Author: Nithin Mohan T K
/// Repository: https://github.com/nithinmohantk/fhir-security
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine("FHIR Security - DPoP Client Example");
        Console.WriteLine("=".PadRight(60, '='));

        // Configuration - Replace with your values
        var config = new FhirSecurityConfig
        {
            FhirServerUrl = Environment.GetEnvironmentVariable("FHIR_SERVER_URL") 
                ?? "https://fhir.example.com/fhir",
            TokenEndpoint = Environment.GetEnvironmentVariable("TOKEN_ENDPOINT") 
                ?? "https://auth.example.com/oauth2/token",
            ClientId = Environment.GetEnvironmentVariable("CLIENT_ID") 
                ?? "my-fhir-client",
            Scope = "system/*.read system/*.write"
        };

        Console.WriteLine($"\nConfiguration:");
        Console.WriteLine($"  FHIR Server: {config.FhirServerUrl}");
        Console.WriteLine($"  Token Endpoint: {config.TokenEndpoint}");
        Console.WriteLine($"  Client ID: {config.ClientId}");

        try
        {
            // Example 1: DPoP Client - Direct Usage
            Console.WriteLine("\n" + "-".PadRight(60, '-'));
            Console.WriteLine("Example 1: Direct DPoP Client Usage");
            Console.WriteLine("-".PadRight(60, '-'));
            
            await DemoDirectDPopClient(config);

            // Example 2: FHIR Security Client - High-Level Usage
            Console.WriteLine("\n" + "-".PadRight(60, '-'));
            Console.WriteLine("Example 2: FHIR Security Client Usage");
            Console.WriteLine("-".PadRight(60, '-'));
            
            await DemoFhirSecurityClient(config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        Console.WriteLine("\n" + "=".PadRight(60, '='));
        Console.WriteLine("Demo Complete");
        Console.WriteLine("=".PadRight(60, '='));
    }

    private static async Task DemoDirectDPopClient(FhirSecurityConfig config)
    {
        using var dpopClient = new DPopClient(config.TokenEndpoint, config.ClientId);
        
        Console.WriteLine("1. Authenticating with DPoP...");
        var accessToken = await dpopClient.AuthenticateAsync(config.Scope);
        Console.WriteLine($"   ✅ Access token obtained (length: {accessToken.Length})");

        Console.WriteLine("\n2. Making authenticated FHIR request...");
        var response = await dpopClient.SendAsync(
            HttpMethod.Get, 
            $"{config.FhirServerUrl}/Patient?_count=5"
        );
        
        Console.WriteLine($"   ✅ Response status: {response.StatusCode}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"   ✅ Response length: {content.Length} bytes");
        }
    }

    private static async Task DemoFhirSecurityClient(FhirSecurityConfig config)
    {
        using var fhirClient = new FhirSecurityClient(
            config.FhirServerUrl,
            config.TokenEndpoint,
            config.ClientId
        );

        Console.WriteLine("1. Authenticating...");
        await fhirClient.AuthenticateAsync(config.Scope);
        Console.WriteLine("   ✅ Authenticated successfully");

        Console.WriteLine("\n2. Searching for patients...");
        var bundle = await fhirClient.SearchAsync<Patient>(new Dictionary<string, string>
        {
            ["_count"] = "5"
        });
        
        if (bundle != null)
        {
            Console.WriteLine($"   ✅ Found {bundle.Entry?.Count ?? 0} patients");
            
            foreach (var entry in bundle.Entry ?? new List<Bundle.EntryComponent>())
            {
                if (entry.Resource is Patient patient)
                {
                    var name = patient.Name?.FirstOrDefault()?.ToString() ?? "Unknown";
                    Console.WriteLine($"      - {patient.Id}: {name}");
                }
            }
        }

        Console.WriteLine("\n3. Reading specific patient (if available)...");
        var firstPatient = bundle?.Entry?.FirstOrDefault()?.Resource as Patient;
        if (firstPatient != null)
        {
            var patient = await fhirClient.ReadAsync<Patient>(firstPatient.Id!);
            if (patient != null)
            {
                Console.WriteLine($"   ✅ Retrieved patient: {patient.Id}");
                Console.WriteLine($"      Name: {patient.Name?.FirstOrDefault()?.ToString() ?? "N/A"}");
                Console.WriteLine($"      Birth Date: {patient.BirthDate ?? "N/A"}");
                Console.WriteLine($"      Gender: {patient.Gender?.ToString() ?? "N/A"}");
            }
        }
        else
        {
            Console.WriteLine("   ⚠️  No patients found to retrieve");
        }
    }
}

/// <summary>
/// Configuration for FHIR Security Client.
/// </summary>
public class FhirSecurityConfig
{
    public string FhirServerUrl { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}
