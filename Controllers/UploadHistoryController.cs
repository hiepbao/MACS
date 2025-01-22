using MACS.Services;
using Microsoft.AspNetCore.Mvc;

namespace MACS.Controllers
{
    public class UploadHistoryController : Controller
    {
        private readonly UploadHistoryService _uploadHistoryService;

        public UploadHistoryController(UploadHistoryService uploadHistoryService)
        {
            _uploadHistoryService = uploadHistoryService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var uploadHistory = await _uploadHistoryService.GetUploadHistoryAsync();

                if (uploadHistory == null || !uploadHistory.Any())
                {
                    ViewBag.Message = "Không có dữ liệu để hiển thị.";
                    return View(); 
                }
                var sortedHistory = uploadHistory.OrderByDescending(record => record.Date).ToList();

                return View(sortedHistory);
            }
            catch
            {
                ViewBag.Message = "Đã xảy ra lỗi khi tải dữ liệu.";
                return View(); 
            }
        }

    }
}
