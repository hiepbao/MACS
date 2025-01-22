using MACS.Models;
using System.Net.Http.Headers;

namespace MACS.Services
{
    public class UploadHistoryService
    {
        private readonly HttpClient _httpClient;
        //private const string ApiBaseUrl = "https://localhost:7279";
        private const string ApiBaseUrl = "https://macsapi.onrender.com";
        public UploadHistoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<FileModel>> GetUploadHistoryAsync()
        {
            var apiUrl = $"{ApiBaseUrl}/api/File/readUploadHistory"; // Đường dẫn API

            // Gọi API và nhận dữ liệu JSON
            var result = await _httpClient.GetFromJsonAsync<List<FileModel>>(apiUrl);

            return result ?? new List<FileModel>(); // Trả về danh sách (hoặc danh sách rỗng nếu null)
        }

        // Phương thức upload file
        public async Task<string> UploadFileAsync(IFormFile file, string token)
        {
            var apiUrl = $"{ApiBaseUrl}/api/File/uploadZip";

            using (var content = new MultipartFormDataContent())
            {
                using (var streamContent = new StreamContent(file.OpenReadStream()))
                {
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(streamContent, "zipFile", file.FileName);

                    // Thêm token vào Header Authorization
                    if (!string.IsNullOrEmpty(token))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }

                    var response = await _httpClient.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        return $"{file.FileName}: Tải lên thành công!";
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        return $"{file.FileName}: Có lỗi xảy ra - {errorContent}";
                    }
                }
            }
        }
    }
}
