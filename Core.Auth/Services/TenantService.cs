using Core.Auth.Database.ORM;
using Core.Auth.Models.Tenant;
using CoreCore.DB.Plugin.Shared.Database;

namespace Core.Auth.Services
{
    public static class TenantService
    {
        public static CreateOrUpdateTenant_Response CreateOrUpdateTenant(CORE_DB_Connection connection, CreateOrUpdateTenant_Request parameter)
        {
            try
            {
                auth_tenant.Model dbTenant;

                if (parameter.ID > 0)
                {
                    dbTenant = auth_tenant.DB.Search(connection, new auth_tenant.Query { auth_tenant_id = parameter.ID }).FirstOrDefault();

                    if (dbTenant == null || dbTenant.auth_tenant_id == 0)
                    {
                        // handle error
                    }
                }
                else
                {
                    dbTenant = new auth_tenant.Model();
                }

                dbTenant.tenant_name = parameter.Name;

                auth_tenant.DB.Save(connection, dbTenant);

                return new CreateOrUpdateTenant_Response
                {
                    ID = dbTenant.tenant_id,
                    Name = dbTenant.tenant_name
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}