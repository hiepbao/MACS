
using Microsoft.EntityFrameworkCore;
using MACSAPI.Data;

namespace MACS.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _dbContext;

        public NotificationService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task NotifyUserTokenAsync(int? userId, string title, string message, string clickActionUrl)
        {
            List<string> tokens;

            if (userId == null || userId == 0)  // Nếu không chọn người dùng
            {
                #nullable disable
                tokens = await _dbContext.TokenRequest
                    //.Where(t => t.Role.ToLower() == "store")
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
    }

        
    

    public class FirebaseConfig
    {
        public string Json { get; set; }
    }
}
