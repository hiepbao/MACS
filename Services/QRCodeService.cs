using System.Net.Http.Headers;
using System.Text.Json;
using MACS.Models;

namespace MACS.Services
{
    public class QRCodeService
    {
        private readonly HttpClient _httpClient;

        public QRCodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<QrCodeResponse> ScanQRCodeAsync(IFormFile qrImage)
        {
            var apiUrl = "https://macsapi.onrender.com/api/QRCode/ScanQR";

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
