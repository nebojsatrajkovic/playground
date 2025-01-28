using Core.Auth.Database.DB.Accounts;
using Core.Auth.Database.ORM;
using Core.Auth.Models.Account;
using Core.Shared.Models;
using Core.Shared.Services;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;

namespace Core.Auth.Services
{
    internal static class AccountService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(AccountService));

        internal static ResultOf CreateOrUpdateAccount(CORE_DB_Connection connection)
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

        internal static ResultOf<ConfirmRegistration_Response> ConfirmRegistration(CORE_DB_Connection connection, ConfirmRegistration_Request parameter)
        {
            ResultOf<ConfirmRegistration_Response> returnValue;

            try
            {
                var confirmationRequest = auth_registration_confirmation.Database.Search(connection, new auth_registration_confirmation.QueryParameter
                {
                    is_deleted = false,
                    token = parameter.Token
                }).FirstOrDefault();

                if (confirmationRequest == null)
                {
                    return new ResultOf<ConfirmRegistration_Response>(CORE_OperationStatus.FAILED, "Failed to find confirmation request for the provided token");
                }

                var account = auth_account.Database.Search(connection, new auth_account.QueryParameter
                {
                    auth_account_id = confirmationRequest.account_refid
                }).FirstOrDefault();

                if (account == null)
                {
                    return new ResultOf<ConfirmRegistration_Response>(CORE_OperationStatus.FAILED, "Account was not found for the provided token");
                }

                if (account.is_verified)
                {
                    confirmationRequest.is_processed = true;
                    confirmationRequest.is_confirmed = true;

                    auth_registration_confirmation.Database.Save(connection, confirmationRequest);

                    return new ResultOf<ConfirmRegistration_Response>(CORE_OperationStatus.SUCCESS, "Account is already verified");
                }

                if (DateTime.Now > confirmationRequest.expires_at)
                {
                    confirmationRequest.is_deleted = true;

                    auth_registration_confirmation.Database.Save(connection, confirmationRequest);

                    var sendRegistrationEmail = SendAccountRegistrationConfirmationEmail(connection, account.auth_account_id, account.email);

                    if (!sendRegistrationEmail.Succeeded)
                    {
                        return new ResultOf<ConfirmRegistration_Response>(sendRegistrationEmail, "Confirmation link has expired, but server failed to send a new one");
                    }

                    return new ResultOf<ConfirmRegistration_Response>(CORE_OperationStatus.FAILED, "Confirmation link has expired, a new one is sent");
                }

                confirmationRequest.is_processed = true;
                confirmationRequest.is_confirmed = true;

                auth_registration_confirmation.Database.Save(connection, confirmationRequest);

                account.is_verified = true;

                auth_account.Database.Save(connection, account);

                returnValue = new ResultOf<ConfirmRegistration_Response>(new ConfirmRegistration_Response
                {
                    IsSuccess = true
                });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to register tenant: ", ex);

                returnValue = new ResultOf<ConfirmRegistration_Response>(ex);
            }

            return returnValue;
        }

        internal static ResultOf ResendRegistrationConfirmationEmail(CORE_DB_Connection connection, ResendRegistrationConfirmationEmail_Request parameter)
        {
            ResultOf returnValue;

            try
            {
                var accounts = auth_account.Database.Search(connection, new auth_account.QueryParameter
                {
                    email = parameter.Email,
                    tenant_id = parameter.TenantID > 0 ? parameter.TenantID : null
                });

                if (accounts == null || accounts.Count == 0)
                {
                    return new ResultOf(CORE_OperationStatus.FAILED, "Failed to find an account with the provided email");
                }

                if (accounts.Count > 1)
                {
                    return new ResultOf(CORE_OperationStatus.FAILED, "You must specify for which tenant you want to resend confirmation email");
                }

                var account = accounts[0];

                if (account.is_verified)
                {
                    return new ResultOf(CORE_OperationStatus.FAILED, "Account has been already verified");
                }

                var sendRegistrationEmail = SendAccountRegistrationConfirmationEmail(connection, account.auth_account_id, account.email);

                if (!sendRegistrationEmail.Succeeded)
                {
                    return new ResultOf(sendRegistrationEmail);
                }

                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to resend account registration confirmation email: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }

        internal static ResultOf<List<TenantForAccount>> GetAccountTenants(CORE_DB_Connection connection, GetTenantsForAccount_Request parameter)
        {
            ResultOf<List<TenantForAccount>> returnValue;

            try
            {
                var tenantsForAccount = Get_Tenants_for_AccountEmail.Invoke(connection.Connection, connection.Transaction, new P_GTfAE { Email = parameter.Email });

                var result = tenantsForAccount?.Select(x => new TenantForAccount
                {
                    TenantID = x.auth_tenant_id,
                    TenantName = x.tenant_name
                }).ToList() ?? [];

                returnValue = new ResultOf<List<TenantForAccount>>(result);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to get tenants for account: ", ex);

                returnValue = new ResultOf<List<TenantForAccount>>(ex);
            }

            return returnValue;
        }

        internal static ResultOf SendAccountRegistrationConfirmationEmail(CORE_DB_Connection connection, int accountId, string email)
        {
            ResultOf returnValue;

            try
            {
                var registration_confirmation = new auth_registration_confirmation.ORM
                {
                    account_refid = accountId,
                    expires_at = DateTime.Now.AddHours(24),
                    token = Guid.NewGuid().ToString(),
                    created_at = DateTime.Now,
                    modified_at = DateTime.Now
                };

                auth_registration_confirmation.Database.Save(connection, registration_confirmation);

                var confirmationLink = "http://localhost:21000/api/registration/confirm-registration?token=" + registration_confirmation.token;

                returnValue = EmailService.SendEmail([email], "Confirm registration with CORE", @"
                    <html>
                    <head>
                    </head>
                    <body>
                        <h1>CORE</h1>
                        Confirm your email via this <a href='" + confirmationLink + @"'>link</a>
                    </body>
                    </html>
                ");
            }
            catch (Exception ex)
            {
                logger.Error("Failed to send account registration confirmation email: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }
    }
}