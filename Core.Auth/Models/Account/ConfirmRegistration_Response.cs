namespace Core.Auth.Models.Account
{
    public class ConfirmRegistration_Response
    {
        public bool IsSuccess { get; set; }
        public bool IsExpired { get; set; }
        public bool IsAlreadyVerified { get; set; }
    }
}