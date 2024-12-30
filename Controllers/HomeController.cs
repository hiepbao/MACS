using MACS.Models;
using MACS.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace MACS.Controllers
{
    public class HomeController : Controller
    {

        private readonly QRCodeService _qrCodeService;
        private readonly HistoryCarService _historyCarService;

        public HomeController(QRCodeService qrCodeService, HistoryCarService historyCarService)
        {
            _qrCodeService = qrCodeService;
            _historyCarService = historyCarService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allHistoryCars = await _historyCarService.GetAllHistoryCarsAsync();
            var waitingCars = await _historyCarService.GetAllHistoryCarsInAsync();

            ViewBag.WaitingCars = waitingCars ?? new List<HistoryCar>();

            if (allHistoryCars == null)
            {
                return View(new List<HistoryCar>());
            }

            return View(allHistoryCars);
        }


        public IActionResult Create()
        {
            return View(new HistoryCar
            {
                GetInDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")), 
                IsCarriedOut = false,
                IsGetOut = false,
                GetOutDate = null,
                GetOutBy = null
            });
        }

        // POST: Tạo mới dữ liệu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HistoryCar historyCar)
        {
            if (!ModelState.IsValid)
            {
                return View(historyCar); 
            }

            // Kiểm tra nếu số thẻ đã có xe vào (IsGetIn = true)
            var existingCarResponse = await _historyCarService.CheckCardHasCarInAsync(historyCar.CardNo);

            if (existingCarResponse)
            {
                    TempData["ErrorMessage"] = "Thẻ này đã có xe vào.";
                    return RedirectToAction("Index");
            }
            
            // Gán giá trị mặc định nếu cần
            historyCar.GetInDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            historyCar.IsCarriedOut = false;
            historyCar.IsGetOut = false;
            historyCar.GetOutDate = null;
            historyCar.GetOutBy = null;
            historyCar.ModifiedBy = null;
            historyCar.IsGetIn = true;
            // Lấy JWT từ cookie
            var jwtToken = HttpContext.Request.Cookies["UserToken"];
            if (!string.IsNullOrEmpty(jwtToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwtToken);

                var fullName = token.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value;
                if (!string.IsNullOrEmpty(fullName))
                {
                    historyCar.GetInBy = fullName;
                    historyCar.CreatedBy = fullName;
                    historyCar.ModifiedBy = fullName;
                }
                else
                {
                    historyCar.GetInBy = "Unknown User"; 
                }
            }
            else
            {
                historyCar.GetInBy = "Unknown User";
            }


            var response = await _historyCarService.CreateHistoryCarAsync(historyCar);

            if (response)
            {
                TempData["SuccessMessage"] = "Cho vào thành công!";
                return RedirectToAction("Index"); 
            }

            ModelState.AddModelError("", "Không thể tạo mới dữ liệu. Vui lòng thử lại.");
            return View(historyCar);
        }

        // GET: Hiển thị form chỉnh sửa
        [HttpGet]
        public async Task<IActionResult> Edit(string cardNo)
        {
            if (string.IsNullOrWhiteSpace(cardNo))
            {
                TempData["ErrorMessage"] = "Mã thẻ không được để trống.";
                return RedirectToAction("Index");
            }

            var historyCar = await _historyCarService.GetHistoryCarByCardNoAsync(cardNo);
            if (historyCar == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy dữ liệu thẻ xe hoặc xe đã ra khỏi bãi.";
                return RedirectToAction("Index");
            }

            return View(historyCar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string cardNo, HistoryCar historyCar)
        {
            if (cardNo != historyCar.CardNo)
            {
                return BadRequest("CardNo không khớp.");
            }

            if (!ModelState.IsValid)
            {
                return View(historyCar);
            }
            // Lấy dữ liệu cũ từ API
            var oldHistoryCar = await _historyCarService.GetHistoryCarByCardNoAsync(cardNo);
            if (oldHistoryCar == null || oldHistoryCar.IsGetOut)
            {
                TempData["ErrorMessage"] = "Không có bản ghi nào phù hợp để chỉnh sửa (đã ra khỏi bãi).";
                return RedirectToAction("Index");
            }

            // Cập nhật ngày chỉnh sửa (ModifiedDate)
            historyCar.ModifiedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            historyCar.GetOutDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
            historyCar.IsCarried = oldHistoryCar.IsCarried;
            historyCar.IsCarriedIn = oldHistoryCar.IsCarriedIn;
            historyCar.IsGetIn = oldHistoryCar.IsGetIn;
            historyCar.IsGetOut = true;
            // Lấy JWT từ cookie
            var jwtToken = HttpContext.Request.Cookies["UserToken"];
            if (!string.IsNullOrEmpty(jwtToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwtToken);

                var fullName = token.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value;
                if (!string.IsNullOrEmpty(fullName))
                {
                    historyCar.GetOutBy = fullName;
                    historyCar.ModifiedBy = fullName;
                }
                else
                {
                    historyCar.GetInBy = "Unknown User"; 
                }
            }
            else
            {
                historyCar.GetInBy = "Unknown User";
            }
            
            var response = await _historyCarService.UpdateHistoryCarAsync(cardNo, historyCar);

            if (response)
            {
                TempData["SuccessMessage"] = "Cho ra thành công!";
                return RedirectToAction("Index"); 
            }

            ModelState.AddModelError("", "Không thể cập nhật dữ liệu. Vui lòng thử lại.");
            return View(historyCar);
        }





        [HttpPost]
        public async Task<IActionResult> ScanQR(IFormFile qrImage)
        {
            if (qrImage == null || qrImage.Length == 0)
            {
                ViewBag.Message = "No image uploaded. Please capture an image.";
                return View("Index");
            }

            try
            {
                var result = await _qrCodeService.ScanQRCodeAsync(qrImage);

                if (result != null && result.Data != null && result.Data is List<string> dataList)
                {
                    // Convert data list to a dictionary
                    var dataDict = dataList
                        .Select(item => item.Split(":"))
                        .Where(parts => parts.Length > 1)
                        .ToDictionary(parts => parts[0].Trim().ToLower(), parts => parts[1].Trim());

                    if (dataDict.ContainsKey("message"))
                    {
                        var message = dataDict["message"].ToLower();

                        if (message == "xuất kho")
                        {
                            TempData["Result"] = Newtonsoft.Json.JsonConvert.SerializeObject(result);
                            return RedirectToAction("Warehouse");
                        }
                        else if (message == "xe ra")
                        {
                            TempData["Result"] = Newtonsoft.Json.JsonConvert.SerializeObject(result); 
                            return RedirectToAction("CheckOut");
                        }
                        else
                        {
                            ViewBag.Message = "Lỗi: QR không hợp lệ.";
                            return View("Index");
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Lỗi: Không tìm thấy trường 'message' trong QR.";
                        return View("Index");
                    }
                }
                else
                {
                    ViewBag.Message = "Lỗi: Không thể đọc dữ liệu từ QR.";
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Đã xảy ra lỗi trong quá trình quét QR: " + ex.Message;
                return View("Index");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
