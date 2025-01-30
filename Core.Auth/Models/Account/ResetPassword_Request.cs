namespace Core.Auth.Models.Account
{
    public class ResetPassword_Request
    {
        public string Token { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}