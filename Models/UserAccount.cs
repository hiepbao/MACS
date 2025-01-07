using System.ComponentModel.DataAnnotations;

namespace MACSAPI.Models
{
    // Models/UserAccount.cs
    public class UserAccount
    {
        [Key]
        public int AccountId { get; set; }
        public int EmployeeId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public bool IsActivated { get; set; }
        public bool Admin { get; set; }
        public string Quote { get; set; }
        public bool IsWebApp { get; set; }
        public string Role { get; set; }
    }

    // Models/LoginRequest.cs
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class UserInfo
    {
        [Key]
        public int AccountId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }

}
