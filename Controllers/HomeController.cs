using MACS.Models;
using MACS.Services;
using MACSAPI.Data;
using MACSAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;
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

        private UserInfo GetUserInfoFromToken()
        {
            var jwtToken = HttpContext.Request.Cookies["UserToken"];
            if (string.IsNullOrEmpty(jwtToken))
                return new UserInfo { FullName = "Unknown User" };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);

            return new UserInfo
            {
                AccountId = int.TryParse(token.Claims.FirstOrDefault(c => c.Type == "AccountId")?.Value, out var accountId) ? accountId : 0, 
                FullName = token.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "Unknown User",
                Role = token.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "User"
            };

        }


        private IActionResult SetErrorMessageAndRedirect(string message, string action)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(action);
        }

        private async Task NotifyUserTokenAsync(int? userId, string title, string message, string clickActionUrl)
        {
            List<string> tokens;

            if (userId == null || userId == 0)  // Nếu không chọn người dùng
            {
                #nullable disable
                tokens = await _dbContext.TokenRequest
                    .Where(t => t.Role.ToLower() == "store")
                    .Select(t => t.Token)
                    .Distinct()
                    .ToListAsync();
                #nullable enable

            }
            else
            {
                var userToken = await _dbContext.TokenRequest
                    .Where(t => t.Id == userId)
                    .Select(t => t.Token)
                    .FirstOrDefaultAsync();  // Lấy token của người dùng được chọn

                tokens = !string.IsNullOrEmpty(userToken) ? new List<string> { userToken } : new List<string>();
            }

            if (tokens.Count == 0)
            {
                Console.WriteLine("No tokens found to send notifications.");
                return;
            }

            // Gửi thông báo cho tất cả các token trong danh sách
            foreach (var token in tokens)
            {
                var messageToSend = new FirebaseAdmin.Messaging.Message()
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = title,
                        Body = message
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "url", clickActionUrl }  // Gửi URL trong dữ liệu tùy chỉnh
                    },
                    Token = token
                    
                };

                try
                {
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
        public async Task<IActionResult> Create()
        {
            var users = await _historyCarService.GetAllUsersAsync();  // Gọi API lấy danh sách user

            ViewBag.UserList = users.Select(u => new SelectListItem
            {
                Value = u.AccountId.ToString(),  // Giá trị ID user
                Text = u.Username         // Tên user hiển thị
            }).ToList();

            return View(new HistoryCar
            {
                GetInDate = DateTime.Now,
                IsCarriedOut = false,
                IsGetOut = false
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HistoryCar historyCar)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Model Error: {error.ErrorMessage}");
                }
                return View(historyCar);
            }


            if (await _historyCarService.CheckCardHasCarInAsync(historyCar.CardNo))
                return SetErrorMessageAndRedirect("Thẻ này đã có xe vào.", "Index");

            historyCar.GetInDate = GetCurrentTime();
            historyCar.IsGetIn = true;
            var userInfo = GetUserInfoFromToken();
            historyCar.GetInBy = historyCar.CreatedBy = userInfo.FullName;  
            if (await _historyCarService.CreateHistoryCarAsync(historyCar))
            {
                try
                {
                    string clickActionUrl = "/Home/Index";  
                    await NotifyUserTokenAsync(historyCar.UserId, "Xe vào", $"Xe mới {historyCar.CardNo} vừa vào hệ thống.", clickActionUrl);
                    TempData["SuccessMessage"] = "Cho xe vào thành công";


                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi gửi thông báo: {ex.Message}";
                }

                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = $"Lỗi khi thêm";
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
            var userInfo = GetUserInfoFromToken();
            historyCar.GetOutBy = userInfo.FullName;

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
                //var jwt = Request.Cookies["UserToken"];
                //if (string.IsNullOrEmpty(jwt))
                //    return Unauthorized("JWT not found.");

                //var handler = new JwtSecurityTokenHandler();
                //var token = handler.ReadJwtToken(jwt);

                //if (token.Claims.FirstOrDefault(c => c.Type == "role")?.Value != "store")
                //    return Forbid("User does not have the required role.");
                var userInfo = GetUserInfoFromToken();
                var existingToken = _dbContext.TokenRequest.FirstOrDefault(t => t.Id == userInfo.AccountId);

                if (existingToken != null)
                {
                    // Nếu token đã tồn tại, cập nhật token thay vì thêm mới
                    existingToken.Token = model.Token;
                    _dbContext.TokenRequest.Update(existingToken);
                    
                }
                else
                {
                    // Nếu chưa tồn tại, thêm mới
                    _dbContext.TokenRequest.Add(new TokenRequest
                    {
                        Id = userInfo.AccountId,  // AccountId làm khóa chính
                        Token = model.Token,
                        Role = userInfo.Role,
                    });
                }
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
