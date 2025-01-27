namespace Core.Auth.Models.Tenant
{
    public class RegisterTenant_Request
    {
        public string TenantName { get; set; } = null!;

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}