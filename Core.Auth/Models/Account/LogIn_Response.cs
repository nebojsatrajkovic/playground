namespace Core.Auth.Models.Account
{
    public class LogIn_Response
    {
        public bool IsSuccess { get; set; }
        public bool IfError_MustSpecifyTenant { get; set; }
        public bool IfError_AccountIsNotVerified { get; set; }
        public bool IfError_InvalidCredentials { get; set; }
    }
}