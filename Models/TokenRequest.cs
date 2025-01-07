namespace MACS.Models
{
    public class TokenRequest
    {
        public int Id { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
    }

    public class FcmToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
