using MACS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace MACS.Controllers
{
    public class UploadController : Controller
    {
        private readonly UploadHistoryService _uploadHistoryService;

        public UploadController(UploadHistoryService uploadHistoryService)
        {
            _uploadHistoryService = uploadHistoryService;
        }

        public async Task<IActionResult> Index()
        {
            await Task.Delay(100);
            return View("Index"); 
           
        }



        [HttpPost]
        public async Task<IActionResult> Index(List<IFormFile> zipFiles)
        {
            var results = new List<string>();

            if (zipFiles == null || zipFiles.Count == 0)
            {
                results.Add("Không có file nào được chọn.");
                ViewBag.Results = results;
                return await Index();
            }

            // Lấy token từ cookie
            var token = HttpContext.Request.Cookies["UserToken"];
            if (string.IsNullOrEmpty(token))
            {
                results.Add("Không tìm thấy thông tin đăng nhập. Vui lòng đăng nhập lại.");
                ViewBag.Results = results;
                return await Index();
            }

            foreach (var zipFile in zipFiles)
            {
                if (!zipFile.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    results.Add($"{zipFile.FileName}: Không phải file .zip hợp lệ.");
                    continue;
                }

                try
                {
                    var result = await _uploadHistoryService.UploadFileAsync(zipFile, token);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    results.Add($"{zipFile.FileName}: Lỗi xảy ra - {ex.Message}");
                }
            }

            ViewBag.Results = results;
            return await Index();
        }
    }
}
