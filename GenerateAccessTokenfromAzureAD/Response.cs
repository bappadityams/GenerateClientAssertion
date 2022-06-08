using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace GenerateAccessTokenfromAzureAD
{
    public class Response
    {
        [JsonPropertyName("access_token")]
        public string? accessToken { get; set; }

      
    }
}
