using MACS.Models;
using MACS.Services;
using MACSAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace MACS.Controllers
{
    public class HomeController : Controller
    {
        private readonly QRCodeService _qrCodeService;
        private readonly HistoryCarService _historyCarService;
        private readonly AppDbContext _dbContext;

        public HomeController(QRCodeService qrCodeService, HistoryCarService historyCarService, AppDbContext dbContext)
        {
            _qrCodeService = qrCodeService;
            _historyCarService = historyCarService;
            _dbContext = dbContext;
        }

        private DateTime GetCurrentTime()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        }

        private string GetUserNameFromToken()
        {
            var jwtToken = HttpContext.Request.Cookies["UserToken"];
            if (string.IsNullOrEmpty(jwtToken))
                return "Unknown User";

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);
            return token.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "Unknown User";
        }

        private IActionResult SetErrorMessageAndRedirect(string message, string action)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(action);
        }

        private async Task NotifyAllTokensAsync(string title, string message)
        {
            // Sử dụng Firebase Admin SDK để gửi thông báo mà không cần access token từ file JSON
            var tokens = await _dbContext.TokenRequest.Select(t => t.Token).Distinct().ToListAsync();
            foreach (var token in tokens)
            {
                // Sử dụng Firebase Messaging để gửi thông báo
                var messageToSend = new FirebaseAdmin.Messaging.Message()
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = message
                    },
                    Token = token
                };

                try
                {
                    // Gửi thông báo qua Firebase Messaging
                    string response = await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(messageToSend);
                    Console.WriteLine($"Successfully sent message: {response}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to token {token}: {ex.Message}");
                }
            }
        }


        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewBag.WaitingCars = await _historyCarService.GetAllHistoryCarsInAsync() ?? new List<HistoryCar>();
            var allHistoryCars = await _historyCarService.GetAllHistoryCarsAsync() ?? new List<HistoryCar>();
            return View(allHistoryCars);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new HistoryCar
            {
                GetInDate = GetCurrentTime(),
                IsCarriedOut = false,
                IsGetOut = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HistoryCar historyCar)
        {
            if (!ModelState.IsValid)
                return View(historyCar);

            if (await _historyCarService.CheckCardHasCarInAsync(historyCar.CardNo))
                return SetErrorMessageAndRedirect("Thẻ này đã có xe vào.", "Index");

            historyCar.GetInDate = GetCurrentTime();
            historyCar.IsGetIn = true;
            historyCar.GetInBy = historyCar.CreatedBy = GetUserNameFromToken();

            if (await _historyCarService.CreateHistoryCarAsync(historyCar))
            {
                try
                {
                    await NotifyAllTokensAsync("Xe vào", $"Xe mới {historyCar.CardNo} vừa vào hệ thống.");
                    TempData["SuccessMessage"] = "Cho xe vào thành công";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi gửi thông báo: {ex.Message}";
                }

                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Không thể tạo mới dữ liệu. Vui lòng thử lại.");
            return View(historyCar);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string cardNo)
        {
            if (string.IsNullOrWhiteSpace(cardNo))
                return SetErrorMessageAndRedirect("Mã thẻ không được để trống.", "Index");

            var historyCar = await _historyCarService.GetHistoryCarByCardNoAsync(cardNo);
            if (historyCar == null)
                return SetErrorMessageAndRedirect("Không tìm thấy dữ liệu thẻ xe hoặc xe đã ra khỏi bãi.", "Index");

            return View(historyCar);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string cardNo, HistoryCar historyCar)
        {
            if (cardNo != historyCar.CardNo)
                return BadRequest("CardNo không khớp.");

            if (!ModelState.IsValid)
                return View(historyCar);

            var oldHistoryCar = await _historyCarService.GetHistoryCarByCardNoAsync(cardNo);
            if (oldHistoryCar == null || oldHistoryCar.IsGetOut)
                return SetErrorMessageAndRedirect("Không có bản ghi phù hợp để chỉnh sửa.", "Index");

            historyCar.ModifiedDate = GetCurrentTime();
            historyCar.GetOutDate = GetCurrentTime();
            historyCar.IsGetOut = true;
            historyCar.GetOutBy = GetUserNameFromToken();

            if (await _historyCarService.UpdateHistoryCarAsync(cardNo, historyCar))
            {
                TempData["SuccessMessage"] = "Cho ra thành công!";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Không thể cập nhật dữ liệu. Vui lòng thử lại.");
            return View(historyCar);
        }

        [HttpPost]
        public IActionResult SaveToken([FromBody] TokenRequest model)
        {
            try
            {
                var jwt = Request.Cookies["UserToken"];
                if (string.IsNullOrEmpty(jwt))
                    return Unauthorized("JWT not found.");

                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);

                if (token.Claims.FirstOrDefault(c => c.Type == "role")?.Value != "store")
                    return Forbid("User does not have the required role.");

                _dbContext.TokenRequest.Add(new TokenRequest
                {
                    Id = Guid.NewGuid(),
                    Token = model.Token
                });
                _dbContext.SaveChanges();

                return Ok("Token saved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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

                if (result?.Data is List<string> dataList)
                {
                    var dataDict = dataList
                        .Select(item => item.Split(":"))
                        .Where(parts => parts.Length > 1)
                        .ToDictionary(parts => parts[0].Trim().ToLower(), parts => parts[1].Trim());

                    if (dataDict.TryGetValue("message", out var message))
                    {
                        return message.ToLower() switch
                        {
                            "xuất kho" => RedirectToAction("Warehouse"),
                            "xe ra" => RedirectToAction("CheckOut"),
                            _ => SetErrorMessageAndRedirect("QR không hợp lệ.", "Index")
                        };
                    }
                }

                return SetErrorMessageAndRedirect("Không thể đọc dữ liệu từ QR.", "Index");
            }
            catch (Exception ex)
            {
                return SetErrorMessageAndRedirect($"Đã xảy ra lỗi: {ex.Message}", "Index");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
