
using MACS.Models;
using MACSAPI.Models;
using System.Text;
using System.Text.Json;

namespace MACS.Services
{
    public class TokenService
    {
        private readonly HttpClient _httpClient;
        //private const string ApiBaseUrl = "https://localhost:7279";
        private const string ApiBaseUrl = "https://macsapi.onrender.com";
        public TokenService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetAllTokensAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Token");

                if (response.IsSuccessStatusCode)
                {
                    var tokenList = await response.Content.ReadFromJsonAsync<List<TokenRequest>>();
                    if (tokenList == null || !tokenList.Any())
                        return new List<string>();

                    return tokenList.Select(t => t.Token).Where(token => !string.IsNullOrEmpty(token)).ToList();
                }

                Console.WriteLine($"Failed to get tokens. Status Code: {response.StatusCode}");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetAllTokensAsync: {ex.Message}");
                return new List<string>();
            }
        }



        public async Task<string> GetUserTokenByIdAsync(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Token/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenRequest>();
                    return tokenResponse?.Token ?? string.Empty;
                }
                else
                {
                    Console.WriteLine($"Error: Failed to get token for user {userId}. Status code: {response.StatusCode}");
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetUserTokenByIdAsync: {ex.Message}");
                return string.Empty;
            }
        }


        public async Task<TokenRequest> GetTokenFMCByIdAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}/api/Token/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<TokenRequest>();
                return token;
            }

            return null;
        }

        public async Task<bool> CreateTokenFMCAsync(TokenRequest tokenRequest)
        {
            var apiUrl = $"{ApiBaseUrl}/api/Token";
            var jsonContent = new StringContent(JsonSerializer.Serialize(tokenRequest), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, jsonContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTokenFMCAsync(TokenRequest tokenRequest)
        {
            var apiUrl = $"{ApiBaseUrl}/api/Token/{tokenRequest.Id}";
            var jsonContent = new StringContent(JsonSerializer.Serialize(tokenRequest), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(apiUrl, jsonContent);
            return response.IsSuccessStatusCode;
        }

        
    }

}
