namespace Core.Auth.Models.Account
{
    public class TriggerForgotPassword_Response
    {
        public bool IsSuccess { get; set; }
        public bool IfError_MustSpecifyTenant { get; set; }
    }
}