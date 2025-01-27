using Core.Auth.Models.Tenant;
using Core.Shared.Models;
using CoreCore.DB.Plugin.Shared.Database;

namespace Core.Auth.Services
{
    internal static class AccountService
    {
        internal static ResultOf CreateOrUpdateAccount(CORE_DB_Connection connection)
        {


            return new ResultOf(CORE_OperationStatus.SUCCESS);
        }
    }
}