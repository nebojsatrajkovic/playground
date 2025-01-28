using Core.Auth.Models.Account;
using Core.Auth.Models.Tenant;
using Core.Auth.Services;
using Core.DB.Plugin.MySQL;
using Core.Shared.Models;
using CoreCore.DB.Plugin.Shared.Database;

namespace Core.Auth
{
    public static partial class AUTH
    {
        private static string? ConnectionString { get; set; }

        public static void ConfigureConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }


        // TODO
        // method for registration of a tenant with it's default account (tenants are actually companies)
        // method to create or update an account - watch out for the master account
        // create account -> decide whether to automatically approve it or he needs to confirm his email address
        // method to delete an account
        // method to deactivate an account
        // method to update tenant data
        // method to login
        // method to logout
    }

    public static partial class AUTH
    {
        public static class Tenant
        {
            public static ResultOf<RegisterTenant_Response> CreateOrUpdateTenant(CORE_DB_Connection connection, RegisterTenant_Request parameter) => TenantService.RegisterTenant(connection, parameter);

            public static ResultOf<RegisterTenant_Response> CreateOrUpdateTenant(RegisterTenant_Request parameter)
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    throw new ArgumentNullException(nameof(ConnectionString), $"Use ConfigureConnectionString method to specify connection string.");
                }

                return DB_Action.ExecuteCommitAction(ConnectionString, dbConnection =>
                {
                    return TenantService.RegisterTenant(dbConnection, parameter);
                });
            }
        }
    }

    public static partial class AUTH
    {
        public static class Account
        {
            public static ResultOf CreateOrUpdateAccount(CORE_DB_Connection connection) => AccountService.CreateOrUpdateAccount(connection);

            public static ResultOf CreateOrUpdateAccount()
            {
                if (string.IsNullOrEmpty(ConnectionString))
                {
                    throw new ArgumentNullException(nameof(ConnectionString), $"Use ConfigureConnectionString method to specify connection string.");
                }

                return DB_Action.ExecuteCommitAction(ConnectionString, dbConnection =>
                {
                    return AccountService.CreateOrUpdateAccount(dbConnection);
                });
            }


            public static ResultOf<ConfirmRegistration_Response> ConfirmRegistration(CORE_DB_Connection connection, ConfirmRegistration_Request parameter) => AccountService.ConfirmRegistration(connection, parameter);

            public static ResultOf ResendRegistrationConfirmationEmail(CORE_DB_Connection connection, ResendRegistrationConfirmationEmail_Request parameter) => AccountService.ResendRegistrationConfirmationEmail(connection, parameter);

            public static ResultOf<List<TenantForAccount>> GetAccountTenants(CORE_DB_Connection connection, GetTenantsForAccount_Request parameter) => AccountService.GetAccountTenants(connection, parameter);
        }
    }
}