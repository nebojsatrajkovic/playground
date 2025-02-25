using Core.Auth.Models.Account;
using Core.Shared.Models;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;

namespace Core.Auth.Services
{
    public static class AccountService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(AccountService));

        public static ResultOf<CreateAccount_Response> CreateAccount(CORE_DB_Connection connection, CreateAccount_Request parameter)
        {
            ResultOf<CreateAccount_Response> returnValue;

            try
            {
                // TODO check if the email already exists

                // TODO check password validity


                // watch out for the master account

                // this method should update only the logged in account

                // if create account -> decide whether to automatically approve it or he needs to confirm his email address

                returnValue = new ResultOf<CreateAccount_Response>(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to create account: ", ex);

                returnValue = new ResultOf<CreateAccount_Response>(ex);
            }

            return returnValue;
        }

        public static ResultOf UpdateAccount(CORE_DB_Connection connection)
        {
            ResultOf returnValue;

            try
            {
                // TODO

                // watch out for the master account

                // this method should update only the logged in account

                // if create account -> decide whether to automatically approve it or he needs to confirm his email address

                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to update account: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }

        public static ResultOf ChangeAccountStatus(CORE_DB_Connection connection)
        {
            ResultOf returnValue;

            try
            {
                // TODO activate or deactivate

                // TODO update history

                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to deactivate account: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }

        public static ResultOf DeleteAccount(CORE_DB_Connection connection)
        {
            ResultOf returnValue;

            try
            {
                // TODO

                // TODO update history

                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to delete account: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }
    }
}