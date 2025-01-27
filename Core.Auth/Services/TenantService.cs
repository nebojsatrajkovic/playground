using Core.Auth.Database.ORM;
using Core.Auth.Models.Tenant;
using Core.Shared.Models;
using Core.Shared.Services;
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

                var sendRegistrationEmail = EmailService.SendEmail("", [parameter.Email], "", "");

                if (!sendRegistrationEmail.Succeeded)
                {
                    return new ResultOf<RegisterTenant_Response>(sendRegistrationEmail);
                }

                // generate confirmation email with token and define expiration (24h)

                // TODO send email confirmation - request user to confirm his email address

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