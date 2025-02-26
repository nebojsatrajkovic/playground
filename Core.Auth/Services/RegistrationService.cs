using Core.Auth.Database.DB.Tenants;
using Core.Auth.Database.ORM;
using Core.Auth.Enumeration;
using Core.Auth.Models.Account;
using Core.Auth.Models.Tenant;
using Core.Shared.Models;
using Core.Shared.Services;
using Core.Shared.Utils;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;

namespace Core.Auth.Services
{
    public static class RegistrationService
    {
        static readonly ILog logger = LogManager.GetLogger(typeof(RegistrationService));

        public static ResultOf<RegisterTenant_Response> RegisterTenant(CORE_DB_Connection connection, RegisterTenant_Request parameter)
        {
            ResultOf<RegisterTenant_Response> returnValue;

            try
            {
                if (!PasswordHelper.IsStrongPassword(parameter.Password))
                {
                    return new ResultOf<RegisterTenant_Response>(CORE_OperationStatus.FAILED, "Password is not strong enough. Password should be at least 8 characters long, contain one uppercase letter, one lowercase letter and one special character.");
                }

                var tenantsForName = Get_Tenants_for_Name.Invoke(connection.Connection, connection.Transaction, new P_GTfN { Name = parameter.TenantName });

                if (tenantsForName != null && tenantsForName.Count > 0)
                {
                    return new ResultOf<RegisterTenant_Response>(CORE_OperationStatus.FAILED, "Requested tenant name already exists, it must be unique.");
                }

                var tenant = new auth_tenant.ORM
                {
                    tenant_name = parameter.TenantName,

                    created_at = DateTime.UtcNow,
                    modified_at = DateTime.UtcNow
                };

                auth_tenant.Database.Save(connection, tenant);

                var account = new auth_account.ORM
                {
                    email = parameter.Email,
                    username = parameter.Email,
                    password_hash = PasswordHasher.Hash(parameter.Password),
                    tenant_refid = tenant.auth_tenant_id,
                    is_main_account_for_tenant = true,

                    created_at = DateTime.UtcNow,
                    modified_at = DateTime.UtcNow
                };

                auth_account.Database.Save(connection, account);

                var sendRegistrationEmail = SendVerificationEmail(connection, EVerificationTokenType.AccountVerification, account.auth_account_id, account.tenant_refid, account.email);

                if (!sendRegistrationEmail.Succeeded)
                {
                    return new ResultOf<RegisterTenant_Response>(sendRegistrationEmail);
                }

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

        public static ResultOf<ConfirmRegistration_Response> ConfirmRegistration(CORE_DB_Connection connection, ConfirmRegistration_Request parameter)
        {
            ResultOf<ConfirmRegistration_Response> returnValue;

            try
            {
                var confirmationRequest = auth_verification_token.Database.Search(connection, new auth_verification_token.QueryParameter
                {
                    is_deleted = false,
                    token = parameter.Token
                }).FirstOrDefault();

                if (confirmationRequest == null)
                {
                    return new ResultOf<ConfirmRegistration_Response>(CORE_OperationStatus.FAILED, "Failed to find confirmation request for the provided token");
                }

                if (confirmationRequest.is_processed || confirmationRequest.is_confirmed)
                {
                    return new ResultOf<ConfirmRegistration_Response>(CORE_OperationStatus.FAILED, "This account confirmation request has already been processed");
                }

                var account = auth_account.Database.Search(connection, new auth_account.QueryParameter
                {
                    auth_account_id = confirmationRequest.account_refid
                }).FirstOrDefault();

                if (account == null)
                {
                    confirmationRequest.is_deleted = true;

                    auth_verification_token.Database.Save(connection, confirmationRequest);

                    connection.CommitAndBeginNewTransaction();

                    return new ResultOf<ConfirmRegistration_Response>(CORE_OperationStatus.FAILED, "Account was not found for the provided token");
                }

                if (account.is_verified)
                {
                    confirmationRequest.is_processed = true;
                    confirmationRequest.is_confirmed = true;

                    auth_verification_token.Database.Save(connection, confirmationRequest);

                    return new ResultOf<ConfirmRegistration_Response>(CORE_OperationStatus.SUCCESS, "Account is already verified");
                }

                if (DateTime.UtcNow > confirmationRequest.expires_at)
                {
                    confirmationRequest.is_deleted = true;

                    auth_verification_token.Database.Save(connection, confirmationRequest);

                    connection.CommitAndBeginNewTransaction();

                    var sendVerificationEmail = SendVerificationEmail(connection, EVerificationTokenType.AccountVerification, account.auth_account_id, account.tenant_refid, account.email);

                    if (!sendVerificationEmail.Succeeded)
                    {
                        return new ResultOf<ConfirmRegistration_Response>(sendVerificationEmail, "Confirmation link has expired, but server failed to send a new one");
                    }

                    return new ResultOf<ConfirmRegistration_Response>(CORE_OperationStatus.FAILED, "Confirmation link has expired, a new one is sent");
                }

                confirmationRequest.is_processed = true;
                confirmationRequest.is_confirmed = true;

                auth_verification_token.Database.Save(connection, confirmationRequest);

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

        public static ResultOf ResendRegistrationConfirmationEmail(CORE_DB_Connection connection, ResendRegistrationConfirmationEmail_Request parameter)
        {
            ResultOf returnValue;

            try
            {
                var accounts = auth_account.Database.Search(connection, new auth_account.QueryParameter
                {
                    email = parameter.Email,
                    tenant_refid = parameter.TenantID > 0 ? parameter.TenantID : null
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

                var sendRegistrationEmail = SendVerificationEmail(connection, EVerificationTokenType.AccountVerification, account.auth_account_id, account.tenant_refid, account.email);

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

        internal static ResultOf SendVerificationEmail(CORE_DB_Connection connection, EVerificationTokenType verificationTokenType, int accountId, int tenantId, string email)
        {
            ResultOf returnValue;

            try
            {
                var registration_confirmation = new auth_verification_token.ORM
                {
                    account_refid = accountId,
                    expires_at = verificationTokenType == EVerificationTokenType.AccountVerification ? DateTime.UtcNow.AddHours(24) : DateTime.UtcNow.AddMinutes(30),
                    token = Guid.NewGuid().ToString(),
                    is_account_verification = verificationTokenType == EVerificationTokenType.AccountVerification,
                    is_forgot_password = verificationTokenType == EVerificationTokenType.ForgotPassword,
                    created_at = DateTime.UtcNow,
                    modified_at = DateTime.UtcNow,
                    tenant_refid = tenantId
                };

                auth_verification_token.Database.Save(connection, registration_confirmation);

                var confirmationLink = string.Empty;

                if (verificationTokenType == EVerificationTokenType.AccountVerification)
                {
                    confirmationLink = "http://localhost:21000/api/registration/confirm-registration?token=" + registration_confirmation.token;

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
                else if (verificationTokenType == EVerificationTokenType.ForgotPassword)
                {
                    confirmationLink = "http://localhost:21000/api/registration/forgot-password?token=" + registration_confirmation.token;

                    returnValue = EmailService.SendEmail([email], "Forgot password request - CORE", @"
                    <html>
                    <head>
                    </head>
                    <body>
                        <h1>CORE</h1>
                        To reset your password use this <a href='" + confirmationLink + @"'>link</a>
                    </body>
                    </html>
                    ");
                }
                else
                {
                    returnValue = new ResultOf(CORE_OperationStatus.ERROR, "Unsupported operation, try again later");
                }
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