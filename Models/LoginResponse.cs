namespace MACS.Models
{
    public class LoginResponse
    {
        public int AccountId { get; set; }
        public int EmployeeId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsActivated { get; set; }
        public bool Admin { get; set; }
        public string Quote { get; set; }
        public bool IsWebApp { get; set; }
    }

    public class UserViewModel
    {
        public string Username { get; set; } = "Guest";
        public string Role { get; set; } = "User";
    }
}
