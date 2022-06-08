using generate_client_assertion_ad;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GenerateAccessTokenfromAzureAD;
public class Program
{
    private static readonly HttpClient client = new HttpClient();
    static void Main(string[] args)
    {
        var cert = new X509Certificate2("certificate.pfx", "password");

        var now = DateTime.UtcNow;
        var clientId = "ef52e743-741e-4dc7-8966-c2eee2604ece";
        var tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";

        generate_client_assertion_ad.Program generateClientAssertion = new generate_client_assertion_ad.Program();

        var assertion = generate_client_assertion_ad.Program.GetSignedClientAssertion(cert, tenantId, clientId);
        var accessToken =ProcessRepositories(assertion);
        System.Console.WriteLine(accessToken);

    }
    private static string? ProcessRepositories(string assertion)
    {
        var body = new Dictionary<string, string>()
                        {
                            { "client_id", "ef52e743-741e-4dc7-8966-c2eee2604ece"},
                            { "grant_type", "client_credentials" },
                            { "scope", "https://graph.microsoft.com/.default" },
                            { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
                            { "client_assertion", assertion }
                        };
        
        var url = "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/oauth2/v2.0/token";
        var response = client.PostAsync(url, new FormUrlEncodedContent(body));
        var result = response.Result.Content.ReadAsStringAsync().Result;
        Response? token = JsonSerializer.Deserialize<Response>
            (result);
        return token?.accessToken;
    }
}