namespace Core.Auth.Models.Account
{
    public class TenantForAccount
    {
        public int TenantID { get; set; }
        public string TenantName { get; set; } = null!;
    }
}