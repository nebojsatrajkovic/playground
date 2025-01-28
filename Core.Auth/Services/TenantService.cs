using Core.Auth.Database.DB.Tenants;
using Core.Auth.Database.ORM;
using Core.Auth.Models.Tenant;
using Core.Shared.Models;
using Core.Shared.Utils;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;

namespace Core.Auth.Services
{
    internal static class TenantService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(TenantService));

        internal static ResultOf<RegisterTenant_Response> RegisterTenant(CORE_DB_Connection connection, RegisterTenant_Request parameter)
        {
            ResultOf<RegisterTenant_Response> returnValue;

            try
            {
                var tenantsForName = Get_Tenants_for_Name.Invoke(connection.Connection, connection.Transaction, new P_GTfN { Name = parameter.TenantName });

                if (tenantsForName != null && tenantsForName.Count > 0)
                {
                    return new ResultOf<RegisterTenant_Response>(CORE_OperationStatus.FAILED, "Requested tenant name already exists, it must be unique.");
                }

                var tenant = new auth_tenant.ORM
                {
                    tenant_name = parameter.TenantName,

                    created_at = DateTime.Now,
                    modified_at = DateTime.Now
                };

                auth_tenant.Database.Save(connection, tenant);

                var account = new auth_account.ORM
                {
                    email = parameter.Email,
                    username = parameter.Email,
                    password_hash = PasswordHasher.Hash(parameter.Password),
                    tenant_id = tenant.auth_tenant_id,
                    is_main_account_for_tenant = true,

                    created_at = DateTime.Now,
                    modified_at = DateTime.Now
                };

                auth_account.Database.Save(connection, account);

                var sendRegistrationEmail = AccountService.SendAccountRegistrationConfirmationEmail(connection, account.auth_account_id, account.email);

                if (!sendRegistrationEmail.Succeeded)
                {
                    return new ResultOf<RegisterTenant_Response>(sendRegistrationEmail);
                }

                returnValue = new ResultOf<RegisterTenant_Response>(new RegisterTenant_Response
                {
                    TenantID = tenant.auth_tenant_id,
                    User_AccountID = account.auth_account_id
                });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to register tenant: ", ex);

                returnValue = new ResultOf<RegisterTenant_Response>(ex);
            }

            return returnValue;
        }
    }
}