using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using IdentityModel;
using Microsoft.IdentityModel.Tokens;

namespace csharp_5_jwt_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var cert = new X509Certificate2("certificate.pfx", "password");

            var now = DateTime.UtcNow;
            var clientId = "ef52e743-741e-4dc7-8966-c2eee2604ece";

            var audience = "https://login.microsoftonline.com/72f988bf-86f1-41af-91ab-2d7cd011db47/v2.0";
            var issuer = clientId;
            var subject = clientId;
            var jti = Guid.NewGuid().ToString(); // Needs to be a unique value each time
            var issuedAt = now.ToEpochTime().ToString();
            var notBefore = now;
            var expiresAt = now + TimeSpan.FromSeconds(60); // Should be a small value after now


            var token = new JwtSecurityToken(
                issuer,
                audience,
                new[]
                {
                    new Claim(JwtClaimTypes.JwtId, jti),
                    new Claim(JwtClaimTypes.Subject, subject),
                    new Claim(JwtClaimTypes.IssuedAt, issuedAt, ClaimValueTypes.Integer64),
                },
                notBefore,
                expiresAt,
                new SigningCredentials(
                    new X509SecurityKey(cert),
                    SecurityAlgorithms.RsaSha256
                )
            );
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenString = tokenHandler.WriteToken(token);

            System.Console.WriteLine(tokenString);
        }
    }
}
