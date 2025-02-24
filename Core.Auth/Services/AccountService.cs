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

    // TODO method to create or update an account - watch out for the master account
    // TODO create account -> decide whether to automatically approve it or he needs to confirm his email address
    // TODO method to delete an account
    // TODO method to deactivate an account
    // TODO method to update tenant data

    // TODO implement password validity check - to satisfy security criterias
    // TODO implement a way to terminate user's session and update the cache (when it's done via database)
}