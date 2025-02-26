namespace Core.Auth.Models.Account
{
    public class CreateAccount_Request
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsVerified { get; set; }
    }
}