
using Microsoft.EntityFrameworkCore;
using MACSAPI.Data;

namespace MACS.Services
{
    public class NotificationService
    {
        private readonly TokenService _tokenService;

        public NotificationService(TokenService tokenService)
        {
            _tokenService = tokenService;
        }
        public async Task NotifyUserTokenAsync(int? userId, string title, string message, string clickActionUrl)
        {
            List<string> tokens = new List<string>();

            if (userId == null || userId == 0)  // Nếu không chọn người dùng
            {
                tokens = await _tokenService.GetAllTokensAsync();
            }
            else
            {
                var userToken = await _tokenService.GetUserTokenByIdAsync(userId.Value);
                tokens.Add(userToken);
            }

            Console.WriteLine($"Total tokens to send notification: {tokens.Count}");

            var tasks = tokens.Select(async token =>
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending message to token {token}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
        }

    }




    public class FirebaseConfig
    {
        public string Json { get; set; }
    }
}
