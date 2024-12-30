using System.Net.Http.Headers;
using System.Text.Json;

namespace MACS.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        //private const string ApiBaseUrl = "https://localhost:7279";
        private const string ApiBaseUrl = "https://macsapi.onrender.com";
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Username and password are required.");
            }

            // Tạo URL với query string
            var url = $"{ApiBaseUrl}/api/Auth/login?user={Uri.EscapeDataString(username)}&pass={Uri.EscapeDataString(password)}";

            // Gửi yêu cầu GET đến API
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Đọc dữ liệu phản hồi từ API
                var responseContent = await response.Content.ReadAsStringAsync();

                // Deserialize phản hồi để lấy chuỗi JWT
                var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var token = responseData.GetProperty("token").GetString();

                if (!string.IsNullOrEmpty(token))
                {
                    return token;
                }
                else
                {
                    throw new Exception("Token không tồn tại trong phản hồi.");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                var errorMessage = JsonSerializer.Deserialize<JsonElement>(errorResponse).GetProperty("message").GetString();
                throw new UnauthorizedAccessException(errorMessage ?? "Unauthorized access.");
            }
            else
            {
                throw new Exception($"Có lỗi xảy ra. HTTP Status Code: {response.StatusCode}");
            }
        }
    }
}
