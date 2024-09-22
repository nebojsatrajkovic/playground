namespace Core.Auth.Models.Tenant
{
    public class CreateOrUpdateTenant_Response
    {
        public int ID { get; set; }
        public string Name { get; set; } = null!;
    }
}