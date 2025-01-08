using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core
{
    public class CloudflareSiteVerifyClient
    {
        private readonly HttpClient _httpClient;
        // This is the App Id provided by the dashboard that identifies this Calls Application.
        private string appId = "asd";
        private string baseUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";
        // DO NOT USE YOUR SECRET IN THE BROWSER FOR PRODUCTION. It should be kept and used server-side.
        private string secret = "asd";
        public CloudflareSiteVerifyClient()
        {
            _httpClient = new HttpClient();
        }
        public class SiteverifyResponse
        {
            public bool Success { set; get; }

            [JsonPropertyName("error-codes")]
            public string[] ErrorCodes { set; get; } = [];

            public string[] Messages { set; get; } = [];

        }
        public async Task<bool> VerifyAsync(HttpRequest request, string cfTurnstileResponse)
        {
            var token = cfTurnstileResponse;
            string ip = string.Empty;
            StringValues values;
            if (request.Headers.TryGetValue("CF-Connecting-IP", out values))
            {
                ip = values.First()!;
            }

            var serializedBody = JsonSerializer.Serialize(new { secret = secret, response = token, remoteip = ip });
            var content = new StringContent(serializedBody, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(baseUrl, content);

            var siteverifyResponse = await response.Content.ReadFromJsonAsync<SiteverifyResponse>();
            if (siteverifyResponse != null)
            {
                return siteverifyResponse.Success;
            }
            return false;
        }
    }

}
