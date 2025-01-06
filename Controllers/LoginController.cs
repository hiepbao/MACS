using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MACS.Models;
using Microsoft.AspNetCore.Http;
using MACS.Services;

namespace MACS.Controllers
{
    public class LoginController : Controller
    {
        private readonly AuthService _authService;

        public LoginController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                ModelState.AddModelError("Username", "The username field is required.");
                return View();
            }

            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("Password", "The password field is required.");
                return View();
            }

            try
            {
                // Gọi AuthService để xử lý đăng nhập
                var token = await _authService.LoginAsync(username, password);

                // Lưu JWT vào Cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = false, // Cho phép JavaScript truy cập
                    Secure = false,   // Đặt false nếu đang chạy trên localhost, true nếu dùng HTTPS
                    SameSite = SameSiteMode.Lax, // Lax phù hợp hơn để tránh lỗi CORS trong một số trường hợp
                    Expires = DateTime.UtcNow.AddHours(1) // Thời gian sống của cookie
                };

                HttpContext.Response.Cookies.Append("UserToken", token, cookieOptions);

                TempData["Message"] = "Đăng nhập thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Đã xảy ra lỗi: {ex.Message}");
            }

            // Trả về view nếu có lỗi
            return View();
        }

    }
}
