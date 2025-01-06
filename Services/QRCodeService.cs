using System.Net.Http.Headers;
using System.Text.Json;
using MACS.Models;

namespace MACS.Services
{
    public class QRCodeService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://localhost:7279";
        //private const string ApiBaseUrl = "https://macsapi.onrender.com";
        public QRCodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<HistoryCar>> GetHistoryCarsAsync()
        {
            var apiUrl = $"{ApiBaseUrl}/api/HistoryCar";
            List<HistoryCar> historyCars = new List<HistoryCar>();

            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                historyCars = await response.Content.ReadFromJsonAsync<List<HistoryCar>>();
            }
            else
            {
                // Bạn có thể xử lý lỗi hoặc ghi log nếu cần
                throw new HttpRequestException($"Failed to fetch data from API. Status code: {response.StatusCode}");
            }

            return historyCars;
        }
        public async Task<bool> CreateHistoryCarAsync(HistoryCar model)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/api/HistoryCar", model);

                if (response.IsSuccessStatusCode)
                {
                    return true; // Thành công
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Console.WriteLine($"Lỗi khi gọi API: {ex.Message}");
            }

            return false; // Thất bại
        }
        public async Task<QrCodeResponse> ScanQRCodeAsync(IFormFile qrImage)
        {
            var apiUrl = $"{ApiBaseUrl}/api/QRCode/ScanQR";

            using (var content = new MultipartFormDataContent())
            {
                using (var streamContent = new StreamContent(qrImage.OpenReadStream()))
                {
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    content.Add(streamContent, "qrImage", qrImage.FileName);

                    var response = await _httpClient.PostAsync(apiUrl, content);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error scanning QR code. Status: {response.StatusCode}");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserialize JSON response to QrCodeResponse
                    var result = JsonSerializer.Deserialize<QrCodeResponse>(
                        responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    return result ?? throw new Exception("Invalid response from QR Code API.");
                }
            }
        }
    }
}
