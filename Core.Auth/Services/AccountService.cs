using Core.Shared.Models;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;

namespace Core.Auth.Services
{
    public static class AccountService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(AccountService));

        public static ResultOf CreateOrUpdateAccount(CORE_DB_Connection connection)
        {
            ResultOf returnValue;

            try
            {


                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to create or update account: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }
    }
}