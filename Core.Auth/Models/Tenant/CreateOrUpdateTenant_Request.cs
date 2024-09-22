namespace Core.Auth.Models.Tenant
{
    public class CreateOrUpdateTenant_Request
    {
        public int ID { get; set; }
        public string Name { get; set; } = null!;
    }
}