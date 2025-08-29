using AccountServer.Models;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AccountServer.Services
{
    public class GoogleService
    {
        HttpClient _httpClient;

        public GoogleService()
        {
            _httpClient = new HttpClient() { BaseAddress = new System.Uri("https://oauth2.googleapis.com/") };
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<GoogleTokenData?> GetUserTokenData(string idToken)
        {
            // Verify the ID token via Google's tokeninfo endpoint
            HttpResponseMessage response = await _httpClient.GetAsync($"tokeninfo?id_token={idToken}");

            if (!response.IsSuccessStatusCode)
                return null;

            string resultStr = await response.Content.ReadAsStringAsync();
            GoogleTokenData? tokenData = JsonConvert.DeserializeObject<GoogleTokenData>(resultStr);

            return tokenData;
        }
    }
}
