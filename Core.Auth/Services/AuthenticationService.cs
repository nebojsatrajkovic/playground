using Core.Auth.Database.DB.Accounts;
using Core.Auth.Database.ORM;
using Core.Auth.Enumeration;
using Core.Auth.Models.Account;
using Core.Auth.Models.Cache;
using Core.DB.Plugin.MySQL;
using Core.Shared.Configuration;
using Core.Shared.Models;
using Core.Shared.Utils;
using Core.Shared.Utils.ThreadsafeCollections;
using CoreCore.DB.Plugin.Shared.Database;
using log4net;
using Microsoft.AspNetCore.Http;

namespace Core.Auth.Services
{
    public static class AuthenticationService
    {
        static readonly CORE_TS_Dictionary<string, AUTH_CACHE_Session> sessionsCache;
        static readonly Timer cacheRefreshTimer;
        static readonly Timer sessionCleanupTimer;

        static readonly ILog logger = LogManager.GetLogger(typeof(AuthenticationService));

        static AuthenticationService()
        {
            sessionsCache = new(TimeSpan.FromMinutes(10));
            cacheRefreshTimer = new Timer(async _ => await RefreshCache(), null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
            sessionCleanupTimer = new Timer(async _ => await CleanupSessions(), null, TimeSpan.FromHours(12), TimeSpan.FromHours(12));
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
                    created_at = DateTime.UtcNow,
                    modified_at = DateTime.UtcNow,
                    valid_from = DateTime.UtcNow,
                    valid_to = DateTime.UtcNow.AddHours(8),
                    session_token = AUTH_Cookie.GenerateSessionToken(),
                    tenant_refid = account.tenant_refid
                };

                auth_session.Database.Save(connection, session);

                AUTH_Cookie.UpdateCookie(context, session.session_token);

                sessionsCache.AddOrUpdate(session.session_token, new AUTH_CACHE_Session
                {
                    ValidUntil_UTC = session.valid_to,
                    SessionInfo = new AUTH_CACHE_SessionInfo
                    {
                        SessionID = session.auth_session_id,
                        AccountID = session.account_refid,
                        TenantID = session.tenant_refid
                    }
                });

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
                    session.modified_at = DateTime.UtcNow;

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

        public static async Task<ResultOf<SessionInfo>> GetSessionInfo(HttpContext context, CORE_DB_Connection connection)
        {
            try
            {
                var sessionToken = GetSessionToken(context);

                if (string.IsNullOrEmpty(sessionToken))
                {
                    return new ResultOf<SessionInfo>(CORE_OperationStatus.FAILED);
                }

                AUTH_CACHE_Session? session = null;

                var isCached = false;

                if (sessionsCache.TryGetValue(sessionToken, out session) && session != null)
                {
                    isCached = true;
                }
                else
                {
                    var dbSession = (await auth_session.Database.SearchAsync(connection, new auth_session.QueryParameter
                    {
                        is_deleted = false,
                        session_token = sessionToken
                    })).FirstOrDefault();

                    if (dbSession != null)
                    {
                        session = new AUTH_CACHE_Session
                        {
                            ValidUntil_UTC = dbSession.valid_to,
                            SessionInfo = new AUTH_CACHE_SessionInfo
                            {
                                SessionID = dbSession.auth_session_id,
                                AccountID = dbSession.account_refid,
                                TenantID = dbSession.tenant_refid
                            }
                        };
                    }
                }

                if (session == null || session.ValidUntil_UTC <= DateTime.UtcNow)
                {
                    if (session != null)
                    {
                        sessionsCache.Remove(sessionToken);

                        await auth_session.Database.SoftDeleteAsync(connection, new auth_session.ORM
                        {
                            session_token = sessionToken
                        });
                    }

                    AUTH_Cookie.RemoveCookie(context);

                    return new ResultOf<SessionInfo>(CORE_OperationStatus.FAILED);
                }
                else
                {
                    if (!isCached)
                    {
                        sessionsCache.AddOrUpdate(sessionToken, session);
                    }
                }

                return new ResultOf<SessionInfo>(new SessionInfo
                {
                    AccountID = session.SessionInfo.AccountID,
                    TenantID = session.SessionInfo.TenantID,
                    SessionToken = sessionToken
                });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to validate session: ", ex);

                return new ResultOf<SessionInfo>(ex);
            }
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

                if (verificationToken.expires_at < DateTime.UtcNow)
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

        static string GetSessionToken(HttpContext context)
        {
            var sessionToken = string.Empty;

            try
            {
                context.Request.Cookies.TryGetValue(CORE_Configuration.API.AuthKey, out sessionToken);

                if (string.IsNullOrEmpty(sessionToken))
                {
                    context.Request.Headers.TryGetValue(CORE_Configuration.API.AuthKey, out var sessionTokenValue);

                    sessionToken = sessionTokenValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return sessionToken ?? string.Empty;
        }

        static async Task RefreshCache()
        {
            try
            {
                var currentSessionsByTenant = sessionsCache.ToList().GroupBy(x => x.SessionInfo.TenantID).ToDictionary(x => x.Key, x => x.ToList());

                if (currentSessionsByTenant.Count == 0) { return; }

                await DB_Action.ExecuteCommitAction(CORE_Configuration.Database.ConnectionString, async (CORE_DB_Connection DB_Connection) =>
                {
                    foreach (var tenantId in currentSessionsByTenant.Keys)
                    {
                        var sessions = currentSessionsByTenant[tenantId];

                        var sessionIds = sessions.Select(x => (object?)x.SessionInfo.SessionID).ToList();

                        var dbSessions = await auth_session.Database.SearchAsync(DB_Connection, sessionIds);

                        foreach (var dbSession in dbSessions)
                        {
                            if (dbSession.is_deleted || dbSession.valid_to <= DateTime.UtcNow)
                            {
                                sessionsCache.Remove(dbSession.session_token);
                            }
                            else
                            {
                                sessionsCache.AddOrUpdate(dbSession.session_token, new AUTH_CACHE_Session
                                {
                                    ValidUntil_UTC = dbSession.valid_to,
                                    SessionInfo = new AUTH_CACHE_SessionInfo
                                    {
                                        SessionID = dbSession.auth_session_id,
                                        AccountID = dbSession.account_refid,
                                        TenantID = dbSession.tenant_refid
                                    }
                                });
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Error("Failed to refresh sessions cache: ", ex);
            }
        }

        static async Task CleanupSessions()
        {
            try
            {
                await DB_Action.ExecuteCommitAction(CORE_Configuration.Database.ConnectionString, async (CORE_DB_Connection DB_Connection) =>
                {
                    var tenants = await auth_tenant.Database.SearchAsync(DB_Connection, new auth_tenant.QueryParameter
                    {
                        is_deleted = false
                    });

                    foreach (var tenant in tenants)
                    {
                        var expiredSessions = await Get_ExpiredSessions_which_AreNotDeleted.InvokeAsync(DB_Connection.Connection, DB_Connection.Transaction, new P_GESwAND
                        {
                            TenantID = tenant.auth_tenant_id,
                            DateThreshold = DateTime.UtcNow
                        });

                        if (expiredSessions == null || expiredSessions.Count == 0) return;

                        var expiredSessionORMs = expiredSessions.Select(x => new auth_session.ORM { auth_session_id = x.auth_session_id }).ToList();

                        await auth_session.Database.SoftDeleteAsync(DB_Connection, expiredSessionORMs);

                        var ids = expiredSessions.Select(x => x.session_token).ToList();

                        sessionsCache.Remove(ids);
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