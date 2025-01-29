namespace Core.Auth.Models.Account
{
    public class LogIn_Request
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int? TenantID { get; set; }
    }
}