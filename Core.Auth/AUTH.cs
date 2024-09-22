using Core.Auth.Models.Tenant;
using Core.Auth.Services;
using Core.DB.Plugin.MySQL;
using Core.Shared.Models;
using CoreCore.DB.Plugin.Shared.Database;

namespace Core.Auth
{
    public static class AUTH
    {
        private static string? ConnectionString { get; set; }

        public static void ConfigureConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public static ResultOf<CreateOrUpdateTenant_Response> CreateOrUpdateTenant(CORE_DB_Connection connection, CreateOrUpdateTenant_Request parameter) => TenantService.CreateOrUpdateTenant(connection, parameter);

        public static ResultOf<CreateOrUpdateTenant_Response> CreateOrUpdateTenant(CreateOrUpdateTenant_Request parameter)
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                throw new ArgumentNullException(nameof(ConnectionString), $"Use ConfigureConnectionString method to specify connection string.");
            }

            return DB_Action.ExecuteCommitAction(ConnectionString, dbConnection =>
            {
                return TenantService.CreateOrUpdateTenant(dbConnection, parameter);
            });
        }


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
    }
}