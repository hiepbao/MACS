using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MACS.Middlewares
{
    public class UserTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public UserTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Lấy đường dẫn hiện tại
            var path = context.Request.Path.Value.ToLower();

            // Bỏ qua kiểm tra nếu đường dẫn là trang Login
            if (path.Contains("/login"))
            {
                await _next(context);
                return;
            }

            // Kiểm tra cookie UserToken
            if (!context.Request.Cookies.ContainsKey("UserToken"))
            {
                // Chuyển hướng về trang Login nếu không có cookie
                context.Response.Redirect("/login");
                return;
            }

            // Lấy giá trị token từ cookie
            var token = context.Request.Cookies["UserToken"];

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Giải mã JWT
                    var handler = new JwtSecurityTokenHandler();
                    var jwtToken = handler.ReadJwtToken(token);

                    Console.WriteLine(jwtToken);

                    // Kiểm tra thông tin cơ bản (bạn có thể thêm các kiểm tra chữ ký nếu cần)
                    var username = jwtToken.Claims.FirstOrDefault(c => c.Type == "FullName")?.Value ?? "Guest";
                    //var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value ?? "User";

                    // Lưu thông tin vào HttpContext.Items
                    context.Items["Username"] = username;
                    //context.Items["Role"] = role;
                }
                catch
                {
                    // Nếu token không hợp lệ, chuyển hướng về trang Login
                    context.Response.Redirect("/login");
                    return;
                }
            }

            // Chuyển tiếp đến middleware tiếp theo
            await _next(context);
        }
    }
}
