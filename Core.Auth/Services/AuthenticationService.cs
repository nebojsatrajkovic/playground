using Core.Auth.Database.DB.Accounts;
using Core.Auth.Database.ORM;
using Core.Auth.Enumeration;
using Core.Auth.Models.Account;
using Core.DB.Plugin.MySQL;
using Core.Shared.Configuration;
using Core.Shared.Models;
using Core.Shared.Utils;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace Core.Auth.Services
{
    public static class AuthenticationService
    {
        static readonly MemoryCache sessionsCache;
        static Timer sessionCleanupTimer;

        static readonly ILog logger = LogManager.GetLogger(typeof(AuthenticationService));

        static AuthenticationService()
        {
            sessionsCache = new(new MemoryCacheOptions());
            sessionCleanupTimer = new Timer(_ => CleanupSessions(), null, TimeSpan.FromHours(12), TimeSpan.FromHours(12));
        }

        public static ResultOf<LogIn_Response> LogIn(HttpContext context, CORE_DB_Connection connection, LogIn_Request parameter)
        {
            ResultOf<LogIn_Response> returnValue;

            try
            {
                var accounts = auth_account.Database.Search(connection, new auth_account.QueryParameter
                {
                    email = parameter.Email,
                    is_deleted = false,
                    tenant_refid = parameter.TenantID > 0 ? parameter.TenantID : null
                });

                if (accounts == null || accounts.Count == 0)
                {
                    return new ResultOf<LogIn_Response>(CORE_OperationStatus.FAILED, new LogIn_Response { IfError_InvalidCredentials = true });
                }

                if (accounts.Count > 1)
                {
                    return new ResultOf<LogIn_Response>(CORE_OperationStatus.FAILED, new LogIn_Response { IfError_MustSpecifyTenant = true });
                }

                var account = accounts[0];

                if (!account.is_verified)
                {
                    return new ResultOf<LogIn_Response>(CORE_OperationStatus.FAILED, new LogIn_Response { IfError_AccountIsNotVerified = true });
                }

                if (!PasswordHasher.Verify(parameter.Password, account.password_hash))
                {
                    return new ResultOf<LogIn_Response>(CORE_OperationStatus.FAILED, new LogIn_Response { IfError_InvalidCredentials = true });
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
                    tenant_refid = account.tenant_refid
                };

                auth_session.Database.Save(connection, session);

                AUTH_Cookie.UpdateCookie(context, session.session_token);

                sessionsCache.Set(session.session_token, session, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(1) });

                returnValue = new ResultOf<LogIn_Response>(new LogIn_Response { IsSuccess = true });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to log in: ", ex);

                returnValue = new ResultOf<LogIn_Response>(ex);
            }

            return returnValue;
        }

        public static ResultOf LogOut(HttpContext context, CORE_DB_Connection connection)
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

        public static ResultOf<ValidateSession_Response> ValidateSession(HttpContext context, CORE_DB_Connection connection, ValidateSession_Request parameter)
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
                    TenantID = session.tenant_refid
                });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to validate session: ", ex);

                returnValue = new ResultOf<ValidateSession_Response>(ex);
            }

            return returnValue;
        }

        public static ResultOf<TriggerForgotPassword_Response> TriggerForgotPassword(HttpContext context, CORE_DB_Connection connection, TriggerForgotPassword_Request parameter)
        {
            ResultOf<TriggerForgotPassword_Response> returnValue;

            try
            {
                AUTH_Cookie.RemoveCookie(context);

                var accounts = auth_account.Database.Search(connection, new auth_account.QueryParameter
                {
                    email = parameter.Email,
                    tenant_refid = parameter.TenantID > 0 ? parameter.TenantID : null
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

                var sendVerificationEmail = RegistrationService.SendVerificationEmail(connection, EVerificationTokenType.ForgotPassword, account.auth_account_id, account.tenant_refid, account.email);

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

        public static ResultOf<ResetPassword_Response> ResetPassword(CORE_DB_Connection connection, ResetPassword_Request parameter)
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
                    tenant_refid = verificationToken.tenant_refid
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

        public static ResultOf<List<TenantForAccount>> GetAccountTenants(CORE_DB_Connection connection, GetTenantsForAccount_Request parameter)
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