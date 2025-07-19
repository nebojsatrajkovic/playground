namespace Core.Auth.Models.Account
{
    public class SessionInfo
    {
        public int AccountID { get; set; }
        public int TenantID { get; set; }
        public string? SessionToken { get; set; }
    }
}