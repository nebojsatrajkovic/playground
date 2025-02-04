using Core.Auth.Database.DB.Accounts;
using Core.Auth.Database.ORM;
using Core.Auth.Enumeration;
using Core.Auth.Models.Account;
using Core.DB.Plugin.MySQL;
using Core.Shared.Configuration;
using Core.Shared.Models;
using Core.Shared.Services;
using Core.Shared.Utils;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Core.Auth.Services
{
    // TODO implement password validity check - to satisfy security criterias
    // TODO implement a way to terminate user's session and update the cache (when it's done via database)

    internal static class AccountService
    {
        static readonly MemoryCache sessionsCache;
        static Timer sessionCleanupTimer;

        static AccountService()
        {
            sessionsCache = new(new MemoryCacheOptions());
            sessionCleanupTimer = new Timer(_ => CleanupSessions(), null, TimeSpan.FromHours(12), TimeSpan.FromHours(12));
        }

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

                if (DateTime.Now > confirmationRequest.expires_at)
                {
                    confirmationRequest.is_deleted = true;

                    auth_verification_token.Database.Save(connection, confirmationRequest);

                    connection.CommitAndBeginNewTransaction();

                    var sendVerificationEmail = SendVerificationEmail(connection, EVerificationTokenType.AccountVerification, account.auth_account_id, account.tenant_id, account.email);

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

                var sendRegistrationEmail = SendVerificationEmail(connection, EVerificationTokenType.AccountVerification, account.auth_account_id, account.tenant_id, account.email);

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

        internal static ResultOf SendVerificationEmail(CORE_DB_Connection connection, EVerificationTokenType verificationTokenType, int accountId, int tenantId, string email)
        {
            ResultOf returnValue;

            try
            {
                var registration_confirmation = new auth_verification_token.ORM
                {
                    account_refid = accountId,
                    expires_at = verificationTokenType == EVerificationTokenType.AccountVerification ? DateTime.Now.AddHours(24) : DateTime.Now.AddMinutes(30),
                    token = Guid.NewGuid().ToString(),
                    is_account_verification = verificationTokenType == EVerificationTokenType.AccountVerification,
                    is_forgot_password = verificationTokenType == EVerificationTokenType.ForgotPassword,
                    created_at = DateTime.Now,
                    modified_at = DateTime.Now,
                    tenant_id = tenantId
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

        internal static ResultOf<LogIn_Response> LogIn(HttpContext context, CORE_DB_Connection connection, LogIn_Request parameter)
        {
            ResultOf<LogIn_Response> returnValue;

            try
            {
                var result = new LogIn_Response();

                var accounts = auth_account.Database.Search(connection, new auth_account.QueryParameter
                {
                    email = parameter.Email,
                    is_deleted = false,
                    tenant_id = parameter.TenantID > 0 ? parameter.TenantID : null
                });

                if (accounts == null || accounts.Count == 0)
                {
                    result.IfError_InvalidCredentials = true;

                    return new ResultOf<LogIn_Response>(CORE_OperationStatus.FAILED, result);
                }

                if (accounts.Count > 1)
                {
                    result.IfError_MustSpecifyTenant = true;

                    return new ResultOf<LogIn_Response>(CORE_OperationStatus.FAILED, result);
                }

                var account = accounts[0];

                if (!account.is_verified)
                {
                    result.IfError_AccountIsNotVerified = true;

                    return new ResultOf<LogIn_Response>(CORE_OperationStatus.FAILED, result);
                }

                if (!PasswordHasher.Verify(parameter.Password, account.password_hash))
                {
                    result.IfError_InvalidCredentials = true;

                    return new ResultOf<LogIn_Response>(CORE_OperationStatus.FAILED, result);
                }

                auth_session.Database.SoftDelete(connection, new auth_session.QueryParameter
                {
                    account_refid = account.auth_account_id
                });

                var session = new auth_session.ORM
                {
                    account_refid = account.auth_account_id,
                    created_at = DateTime.Now,
                    modified_at = DateTime.Now,
                    valid_from = DateTime.UtcNow,
                    valid_to = DateTime.UtcNow.AddHours(8),
                    session_token = AUTH_Cookie.GenerateSessionToken(),
                    tenant_id = account.tenant_id
                };

                auth_session.Database.Save(connection, session);

                AUTH_Cookie.UpdateCookie(context, session.session_token);

                sessionsCache.Set(session.session_token, session, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(1) });

                result.IsSuccess = true;

                returnValue = new ResultOf<LogIn_Response>(result);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to log in: ", ex);

                returnValue = new ResultOf<LogIn_Response>(ex);
            }

            return returnValue;
        }

        internal static ResultOf LogOut(HttpContext context, CORE_DB_Connection connection)
        {
            ResultOf returnValue;

            try
            {
                var sessionToken = AUTH_Cookie.GetSessionToken(context);

                var session = auth_session.Database.Search(connection, new auth_session.QueryParameter
                {
                    is_deleted = false,
                    session_token = sessionToken
                }).FirstOrDefault();

                if (session != null)
                {
                    session.valid_to = DateTime.UtcNow.AddMinutes(-1);
                    session.is_deleted = true;
                    session.modified_at = DateTime.Now;

                    auth_session.Database.Save(connection, session);

                    sessionsCache.Remove(session.session_token);
                }

                AUTH_Cookie.RemoveCookie(context);

                returnValue = new ResultOf(CORE_OperationStatus.SUCCESS);
            }
            catch (Exception ex)
            {
                logger.Error("Failed to log out: ", ex);

                returnValue = new ResultOf(ex);
            }

            return returnValue;
        }

        internal static ResultOf<ValidateSession_Response> ValidateSession(HttpContext context, CORE_DB_Connection connection, ValidateSession_Request parameter)
        {
            ResultOf<ValidateSession_Response> returnValue;

            try
            {
                if (string.IsNullOrEmpty(parameter.SessionToken))
                {
                    return new ResultOf<ValidateSession_Response>(CORE_OperationStatus.FAILED);
                }

                auth_session.ORM? session = null;

                if (sessionsCache.TryGetValue(parameter.SessionToken, out auth_session.ORM? cachedSession) && cachedSession != null)
                {
                    session = cachedSession;
                }
                else
                {
                    session = auth_session.Database.Search(connection, new auth_session.QueryParameter
                    {
                        is_deleted = false,
                        session_token = parameter.SessionToken
                    }).FirstOrDefault();
                }

                if (session == null || session.valid_to < DateTime.UtcNow)
                {
                    if (session != null)
                    {
                        sessionsCache.Remove(session.session_token);

                        auth_session.Database.SoftDelete(connection, session);
                    }

                    AUTH_Cookie.RemoveCookie(context);

                    return new ResultOf<ValidateSession_Response>(CORE_OperationStatus.FAILED);
                }

                returnValue = new ResultOf<ValidateSession_Response>(new ValidateSession_Response
                {
                    AccountID = session.account_refid,
                    TenantID = session.tenant_id
                });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to validate session: ", ex);

                returnValue = new ResultOf<ValidateSession_Response>(ex);
            }

            return returnValue;
        }

        internal static ResultOf<TriggerForgotPassword_Response> TriggerForgotPassword(HttpContext context, CORE_DB_Connection connection, TriggerForgotPassword_Request parameter)
        {
            ResultOf<TriggerForgotPassword_Response> returnValue;

            try
            {
                AUTH_Cookie.RemoveCookie(context);

                var accounts = auth_account.Database.Search(connection, new auth_account.QueryParameter
                {
                    email = parameter.Email,
                    tenant_id = parameter.TenantID > 0 ? parameter.TenantID : null
                });

                if (accounts == null || accounts.Count == 0)
                {
                    return new ResultOf<TriggerForgotPassword_Response>(CORE_OperationStatus.FAILED, "There is no account for the provided email");
                }

                if (accounts.Count > 1)
                {
                    returnValue = new ResultOf<TriggerForgotPassword_Response>(CORE_OperationStatus.FAILED, new TriggerForgotPassword_Response { IfError_MustSpecifyTenant = true });
                }

                var account = accounts[0];

                var sendVerificationEmail = SendVerificationEmail(connection, EVerificationTokenType.ForgotPassword, account.auth_account_id, account.tenant_id, account.email);

                if (!sendVerificationEmail.Succeeded)
                {
                    return new ResultOf<TriggerForgotPassword_Response>(sendVerificationEmail);
                }

                returnValue = new ResultOf<TriggerForgotPassword_Response>(new TriggerForgotPassword_Response { IsSuccess = true });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to trigger forgot password: ", ex);

                returnValue = new ResultOf<TriggerForgotPassword_Response>(ex);
            }

            return returnValue;
        }

        internal static ResultOf<ResetPassword_Response> ResetPassword(CORE_DB_Connection connection, ResetPassword_Request parameter)
        {
            ResultOf<ResetPassword_Response> returnValue;

            try
            {
                var verificationToken = auth_verification_token.Database.Search(connection, new auth_verification_token.QueryParameter
                {
                    is_deleted = false,
                    token = parameter.Token
                }).FirstOrDefault();

                if (verificationToken == null)
                {
                    return new ResultOf<ResetPassword_Response>(CORE_OperationStatus.FAILED, "Forgot password request is not found");
                }

                if (verificationToken.is_processed || verificationToken.is_confirmed)
                {
                    return new ResultOf<ResetPassword_Response>(CORE_OperationStatus.FAILED, "This forgot password request has already been processed");
                }

                if (verificationToken.expires_at < DateTime.Now)
                {
                    verificationToken.is_deleted = true;

                    auth_verification_token.Database.Save(connection, verificationToken);

                    connection.CommitAndBeginNewTransaction();

                    return new ResultOf<ResetPassword_Response>(CORE_OperationStatus.FAILED, "Forgot password request has expired, please retry");
                }

                var account = auth_account.Database.Search(connection, new auth_account.QueryParameter
                {
                    auth_account_id = verificationToken.account_refid,
                    tenant_id = verificationToken.tenant_id
                }).FirstOrDefault();

                if (account == null)
                {
                    verificationToken.is_deleted = true;

                    auth_verification_token.Database.Save(connection, verificationToken);

                    connection.CommitAndBeginNewTransaction();

                    return new ResultOf<ResetPassword_Response>(CORE_OperationStatus.FAILED, "Account not found for which the forgot password was requested");
                }

                verificationToken.is_processed = true;
                verificationToken.is_confirmed = true;

                auth_verification_token.Database.Save(connection, verificationToken);

                account.password_hash = PasswordHasher.Hash(parameter.Password);

                auth_account.Database.Save(connection, account);

                returnValue = new ResultOf<ResetPassword_Response>(new ResetPassword_Response { IsSuccess = true });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to reset password: ", ex);

                returnValue = new ResultOf<ResetPassword_Response>(ex);
            }

            return returnValue;
        }

        static void CleanupSessions()
        {
            try
            {
                DB_Action.ExecuteCommitAction(CORE_Configuration.Database.ConnectionString, (CORE_DB_Connection DB_Connection) =>
                {
                    var tenants = auth_tenant.Database.Search(DB_Connection, new auth_tenant.QueryParameter
                    {
                        is_deleted = false
                    });

                    foreach (var tenant in tenants)
                    {
                        var expiredSessions = Get_ExpiredSessions_which_AreNotDeleted.Invoke(DB_Connection.Connection, DB_Connection.Transaction, new P_GESwAND
                        {
                            TenantID = tenant.auth_tenant_id,
                            DateThreshold = DateTime.UtcNow
                        });

                        if (expiredSessions != null && expiredSessions.Count > 0)
                        {
                            var expiredSessionORMs = expiredSessions.Select(x => new auth_session.ORM { auth_session_id = x.auth_session_id }).ToList();

                            auth_session.Database.SoftDelete(DB_Connection, expiredSessionORMs);

                            foreach (var expiredSession in expiredSessions)
                            {
                                sessionsCache.Remove(expiredSession.session_token);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to cleanup sessions: ", ex);
            }
        }
    }
}