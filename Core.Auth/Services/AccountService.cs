using Core.Auth.Database.DB.Accounts;
using Core.Auth.Database.ORM;
using Core.Shared.Models;
using Core.Shared.Utils;
using CoreCore.DB.Plugin.Shared.Database;

namespace Core.Auth.Services
{
    internal static class AccountService
    {
        internal static ResultOf CreateOrUpdateAccount(CORE_DB_Connection connection)
        {
            var acc = new auth_account.ORM
            {
                created_at = DateTime.Now,
                email = "nebojsa.trajkovic92+playground@gmail.com",
                is_verified = true,
                password = PasswordHasher.Hash(PasswordGenerator.GenerateRandomPassword(8)),
                tenant_id = 1,
                username = "nebojsa.trajkovic92+playground",
                is_deleted = false,
                modified_at = DateTime.Now
            };

            auth_account.Database.Save(connection, acc);

            auth_account.Database.Search(connection, new auth_account.QueryParameter
            {

            });


            var dbAcc = Get_Accounts_for_ID.Invoke(connection.Connection, connection.Transaction, new P_GAfID { AccountID = 1 });

            return new ResultOf(CORE_OperationStatus.SUCCESS);
        }
    }
}