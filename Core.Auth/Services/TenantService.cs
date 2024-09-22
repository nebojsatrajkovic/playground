using Core.Auth.Database.ORM;
using Core.Auth.Models.Tenant;
using Core.Shared.Models;
using CoreCore.DB.Plugin.Shared.Database;

namespace Core.Auth.Services
{
    internal static class TenantService
    {
        internal static ResultOf<CreateOrUpdateTenant_Response> CreateOrUpdateTenant(CORE_DB_Connection connection, CreateOrUpdateTenant_Request parameter)
        {
            try
            {
                auth_tenant.Model? dbTenant;

                if (parameter.ID > 0)
                {
                    dbTenant = auth_tenant.DB.Search(connection, new auth_tenant.Query { auth_tenant_id = parameter.ID }).FirstOrDefault();

                    if (dbTenant == null)
                    {
                        return new ResultOf<CreateOrUpdateTenant_Response>(CORE_OperationStatus.FAILED, $"Tenant was not found for specified id {parameter.ID}");
                    }
                }
                else
                {
                    dbTenant = new auth_tenant.Model();
                }

                dbTenant.tenant_name = parameter.Name;

                auth_tenant.DB.Save(connection, dbTenant);

                return new ResultOf<CreateOrUpdateTenant_Response>(new CreateOrUpdateTenant_Response
                {
                    ID = dbTenant.auth_tenant_id,
                    Name = dbTenant.tenant_name
                });
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}