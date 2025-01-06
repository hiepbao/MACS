using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace MACS.Services
{
    public class NotificationService
    {
        public NotificationService(IOptions<FirebaseConfig> options)
        {
            var firebaseConfigJson = options.Value.Json;

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromJson(firebaseConfigJson)
                    });

                    Console.WriteLine("Khởi tạo FirebaseApp thành công.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi khởi tạo Firebase: {ex.Message}");
                throw;
            }
        }

        public async Task SendNotificationAsync(string deviceToken, string title, string body)
        {
            try
            {
                var message = new Message
                {
                    Token = deviceToken,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    }
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                Console.WriteLine($"Gửi thông báo thành công: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi gửi thông báo: {ex.Message}");
            }
        }
    }

    public class FirebaseConfig
    {
        public string Json { get; set; }
    }
}
