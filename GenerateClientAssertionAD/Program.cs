using System.Collections.Generic;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace generate_client_assertion_ad;
public class Program
{
    public static void Main(string[] args)
    {
        var cert = new X509Certificate2("certificate.pfx", "password");

        var now = DateTime.UtcNow;
        var clientId = "ef52e743-741e-4dc7-8966-c2eee2604ece";
        var tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47";        

        var assertion = GetSignedClientAssertion(cert, tenantId, clientId);
        System.Console.WriteLine(assertion);
    }

    static IDictionary<string, object> GetClaims(string tenantId, string clientId)
    {
        //aud = https://login.microsoftonline.com/ + Tenant ID + /v2.0
        string aud = $"https://login.microsoftonline.com/{tenantId}/v2.0";

        string ConfidentialClientID = clientId; //client id 00000000-0000-0000-0000-000000000000
        const uint JwtToAadLifetimeInSeconds = 60 * 10; // Ten minutes
        DateTimeOffset validFrom = DateTimeOffset.UtcNow;
        DateTimeOffset validUntil = validFrom.AddSeconds(JwtToAadLifetimeInSeconds);

        return new Dictionary<string, object>()
                {
                    { "aud", aud },
                    { "exp", validUntil.ToUnixTimeSeconds() },
                    { "iss", ConfidentialClientID },
                    { "jti", Guid.NewGuid().ToString() },
                    { "nbf", validFrom.ToUnixTimeSeconds() },
                    { "sub", ConfidentialClientID }
                };
    }


    static string Base64UrlEncode(byte[] arg)
    {
        char Base64PadCharacter = '=';
        char Base64Character62 = '+';
        char Base64Character63 = '/';
        char Base64UrlCharacter62 = '-';
        char Base64UrlCharacter63 = '_';

        string s = Convert.ToBase64String(arg);
        s = s.Split(Base64PadCharacter)[0]; // RemoveAccount any trailing padding
        s = s.Replace(Base64Character62, Base64UrlCharacter62); // 62nd char of encoding
        s = s.Replace(Base64Character63, Base64UrlCharacter63); // 63rd char of encoding

        return s;
    }

    public static string GetSignedClientAssertion(X509Certificate2 certificate, string tenantId, string clientId)

    {
        // Get the RSA with the private key, used for signing.
        var rsa = certificate.GetRSAPrivateKey();

        //alg represents the desired signing algorithm, which is SHA-256 in this case
        //x5t represents the certificate thumbprint base64 url encoded
        var header = new Dictionary<string, string>()
                        {
                            { "alg", "RS256"},
                            { "typ", "JWT" },
                            { "x5t", Base64UrlEncode(certificate.GetCertHash()) }
                        };

        //Please see the previous code snippet on how to craft claims for the GetClaims() method
        var claims = GetClaims(tenantId, clientId);

        var headerBytes = JsonSerializer.SerializeToUtf8Bytes(header);
        var claimsBytes = JsonSerializer.SerializeToUtf8Bytes(claims);
        string token = Base64UrlEncode(headerBytes) + "." + Base64UrlEncode(claimsBytes);

        string signature = Base64UrlEncode(rsa.SignData(Encoding.UTF8.GetBytes(token), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1));
        string signedClientAssertion = string.Concat(token, ".", signature);
        return signedClientAssertion;
    }
}

