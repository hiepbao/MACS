using MACS.Models;
using System.Text;
using System.Text.Json;

namespace MACS.Services
{
    public class HistoryCarService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://localhost:7279";
        //private const string ApiBaseUrl = "https://macsapi.onrender.com";
        public HistoryCarService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<HistoryCar>> GetAllHistoryCarsAsync()
        {
            var apiUrl = $"{ApiBaseUrl}/api/HistoryCar";
            var historyCars = await _httpClient.GetFromJsonAsync<List<HistoryCar>>(apiUrl);
            return historyCars ?? new List<HistoryCar>();
        }
        public async Task<List<HistoryCar>> GetAllHistoryCarsInAsync()
        {
            var apiUrl = $"{ApiBaseUrl}/api/HistoryCar/GetHistoryCarsIn";
            var historyCars = await _httpClient.GetFromJsonAsync<List<HistoryCar>>(apiUrl);
            return historyCars ?? new List<HistoryCar>();
        }

        public async Task<HistoryCar?> GetHistoryCarByCardNoAsync(string cardNo)
        {
            var apiUrl = $"{ApiBaseUrl}/api/HistoryCar/byCardno/{cardNo}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var historyCars = await response.Content.ReadFromJsonAsync<List<HistoryCar>>();
            return historyCars?.FirstOrDefault(h => !h.IsGetOut);
        }

        public async Task<bool> CreateHistoryCarAsync(HistoryCar historyCar)
        {
            var apiUrl = $"{ApiBaseUrl}/api/HistoryCar";
            var jsonContent = new StringContent(JsonSerializer.Serialize(historyCar), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(apiUrl, jsonContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateHistoryCarAsync(string cardNo, HistoryCar updatedHistoryCar)
        {
            var apiUrl = $"{ApiBaseUrl}/api/HistoryCar/edit/{cardNo}";
            var jsonContent = new StringContent(JsonSerializer.Serialize(updatedHistoryCar), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(apiUrl, jsonContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CheckCardHasCarInAsync(string cardNo)
        {
            var apiUrl = $"{ApiBaseUrl}/api/HistoryCar/byCardno/{cardNo}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var historyCars = await response.Content.ReadFromJsonAsync<List<HistoryCar>>();
            return historyCars != null && historyCars.Any(car => car.IsGetIn && !car.IsGetOut);
        }
    }

}
