using Core.Shared.Configuration;
using Core.Shared.Database;
using Core.Shared.ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Core.Shared.Controllers
{
    public class AbstractController : ControllerBase
    {
        readonly ILogger logger;
        string _sessionToken = null!;
        CORE_DB_Connection _DB_Connection = null!;
        readonly ICORE_Configuration coreConfiguration = null!;

        protected AbstractController(ILogger logger, ICORE_Configuration coreConfiguration)
        {
            this.logger = logger;
            this.coreConfiguration = coreConfiguration;
        }

        public CORE_DB_Connection DB_Connection
        {
            get
            {
                return _DB_Connection;
            }
            set
            {
                _DB_Connection = value;
            }
        }

        public string SessionToken
        {
            get
            {
                return _sessionToken;
            }
            set
            {
                _sessionToken = value;
            }
        }

        private void _ExecuteCommitAction(Action action, bool authenticate)
        {
            try
            {
                using var dbConnection = new CORE_DB_Connection(coreConfiguration.Database.ConnectionString);

                _DB_Connection = dbConnection;

                if (authenticate)
                {
                    try
                    {
                        if (!_VerifySession(_DB_Connection))
                        {
                            throw new CORE_UnauthenticatedException("Unauthenticated request detected!");
                        }

                        _sessionToken = _GetSessionToken();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);

                        throw;
                    }
                }

                action();

                _DB_Connection?.Dispose();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to execute {action.Method.Name}");
                _DB_Connection?.RollBack();
                _DB_Connection?.Dispose();
                throw;
            }
        }

        private T _ExecuteCommitAction<T>(Func<T> action, bool authenticate)
        {
            using var dbConnection = new CORE_DB_Connection(coreConfiguration.Database.ConnectionString);

            _DB_Connection = dbConnection;

            try
            {
                if (authenticate)
                {
                    try
                    {
                        if (!_VerifySession(_DB_Connection))
                        {
                            throw new CORE_UnauthenticatedException("Unauthenticated request detected!");
                        }

                        _sessionToken = _GetSessionToken();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);

                        throw;
                    }
                }

                var result = action();

                _DB_Connection.Commit();

                _DB_Connection?.Dispose();

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to execute {action.Method.Name}");
                _DB_Connection?.RollBack();
                _DB_Connection?.Dispose();
                throw;
            }
        }

        #region commit with auth

        protected void ExecuteCommitAction(Action action)
        {
            _ExecuteCommitAction(action, true);
        }

        protected T ExecuteCommitAction<T>(Func<T> action)
        {
            return _ExecuteCommitAction(action, true);
        }

        protected void ExecuteCommitActionTask(Action action)
        {
            Task.Factory.StartNew(() =>
            {
                _ExecuteCommitAction(action, true);
            });
        }

        protected Task<T> ExecuteCommitActionTask<T>(Func<T> action)
        {
            return Task.Factory.StartNew(() =>
            {
                return _ExecuteCommitAction(action, true);
            });
        }

        #endregion commit with auth

        #region commit with no auth

        protected void ExecuteUnauthenticatedCommitAction(Action action)
        {
            _ExecuteCommitAction(action, false);
        }

        protected T ExecuteUnauthenticatedCommitAction<T>(Func<T> action)
        {
            return _ExecuteCommitAction(action, false);
        }

        protected void ExecuteUnauthenticatedCommitActionTask(Action action)
        {
            Task.Factory.StartNew(() =>
            {
                _ExecuteCommitAction(action, false);
            });
        }

        protected Task<T> ExecuteUnauthenticatedCommitActionTask<T>(Func<T> action)
        {
            return Task.Factory.StartNew(() =>
            {
                return _ExecuteCommitAction(action, false);
            });
        }

        #endregion commit with no auth

        internal bool _VerifySession(CORE_DB_Connection dbConnection)
        {
            var isAuthenticated = false;

            try
            {
                var sessionToken = _GetSessionToken();

                if (!string.IsNullOrEmpty(sessionToken))
                {

                }

                // TODO implement
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return isAuthenticated;
        }

        internal string _GetSessionToken()
        {
            var sessionToken = string.Empty;

            try
            {
                HttpContext.Request.Cookies.TryGetValue(CORE_Configuration.AuthKey, out sessionToken);

                if (string.IsNullOrEmpty(sessionToken))
                {
                    HttpContext.Request.Headers.TryGetValue(CORE_Configuration.AuthKey, out var sessionTokenValue);

                    sessionToken = sessionTokenValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return sessionToken ?? string.Empty;
        }
    }
}