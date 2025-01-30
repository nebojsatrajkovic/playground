namespace Core.Auth.Models.Account
{
    public class TriggerForgotPassword_Request
    {
        public string Email { get; set; } = null!;
        public int? TenantID { get; set; }
    }
}