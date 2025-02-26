using Core.Auth.Database.DB.Accounts;
using Core.Auth.Database.ORM;
using Core.Auth.Enumeration;
using Core.Auth.Models.Account;
using Core.Shared.Models;
using Core.Shared.Utils;
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
                var dbAccountsForEmail = Get_Accounts_for_Email.Invoke(connection.Connection, connection.Transaction, new P_GAfE
                {
                    Email = parameter.Email,
                    TenantID = connection.TenantID
                });

                if (dbAccountsForEmail != null && dbAccountsForEmail.Count > 0)
                {
                    return new ResultOf<CreateAccount_Response>(CORE_OperationStatus.FAILED, $"Failed to create an account with email {parameter.Email} since it already exists for tenant with id {connection.TenantID}");
                }

                if (!PasswordHelper.IsStrongPassword(parameter.Password))
                {
                    return new ResultOf<CreateAccount_Response>(CORE_OperationStatus.FAILED, "Password is not strong enough. Password should be at least 8 characters long, contain one uppercase letter, one lowercase letter and one special character.");
                }

                var account = new auth_account.ORM
                {
                    email = parameter.Email,
                    username = parameter.Email,
                    password_hash = PasswordHasher.Hash(parameter.Password),
                    tenant_refid = connection.TenantID,
                    created_at = DateTime.UtcNow,
                    modified_at = DateTime.UtcNow,
                    is_verified = parameter.IsVerified
                };

                auth_account.Database.Save(connection, account);

                if (!parameter.IsVerified)
                {
                    var sendRegistrationEmail = RegistrationService.SendVerificationEmail(connection, EVerificationTokenType.AccountVerification, account.auth_account_id, account.tenant_refid, account.email);

                    if (!sendRegistrationEmail.Succeeded)
                    {
                        return new ResultOf<CreateAccount_Response>(sendRegistrationEmail);
                    }
                }

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