using Core.Shared.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Shared
{
    public class AbstractController : ControllerBase
    {
        private IHost _host;
        private ILogger logger;
        private string _sessionToken = null!;

        protected AbstractController(IHost host, ILogger logger)
        {
            _host = host;
            this.logger = logger;
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

        //private void _ExecuteCommitAction(Action action, bool authenticate)
        //{
        //    var context = _host.Services.CreateScope().ServiceProvider.GetRequiredService<PlaygroundContext>();

        //    using var dbContextTransaction = context.Database.BeginTransaction();
        //    try
        //    {
        //        if (authenticate)
        //        {
        //            try
        //            {
        //                if (!_VerifySession(context))
        //                {
        //                    throw new CORE_UnauthenticatedException("Unauthenticated request detected!");
        //                }

        //                _sessionToken = _GetSessionToken();
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine(ex);

        //                throw;
        //            }
        //        }

        //        _DBContext = context;

        //        action();

        //        dbContextTransaction.Commit();
        //    }
        //    catch (Exception ex)
        //    {
        //        dbContextTransaction.Rollback();
        //        logger.LogError(ex, ex.Message);
        //        throw;
        //    }
        //}

        //private T _ExecuteCommitAction<T>(Func<T> action, bool authenticate)
        //{
        //    var context = _host.Services.CreateScope().ServiceProvider.GetRequiredService<PlaygroundContext>();

        //    using var dbContextTransaction = context.Database.BeginTransaction();
        //    try
        //    {
        //        if (authenticate)
        //        {
        //            try
        //            {
        //                if (!_VerifySession(context))
        //                {
        //                    throw new CORE_UnauthenticatedException("Unauthenticated request detected!");
        //                }

        //                _sessionToken = _GetSessionToken();
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine(ex);

        //                throw;
        //            }
        //        }

        //        _DBContext = context;

        //        var result = action();

        //        dbContextTransaction.Commit();

        //        context.Database.CloseConnection();

        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        dbContextTransaction.Rollback();
        //        context.Database.CloseConnection();
        //        logger.LogError(ex, ex.Message);
        //        throw;
        //    }
        //}

        #region commit with auth

        //protected void ExecuteCommitAction(Action action)
        //{
        //    _ExecuteCommitAction(action, true);
        //}

        //protected T ExecuteCommitAction<T>(Func<T> action)
        //{
        //    return _ExecuteCommitAction(action, true);
        //}

        //protected void ExecuteCommitActionTask(Action action)
        //{
        //    Task.Factory.StartNew(() =>
        //    {
        //        _ExecuteCommitAction(action, true);
        //    });
        //}

        //protected Task<T> ExecuteCommitActionTask<T>(Func<T> action)
        //{
        //    return Task.Factory.StartNew(() =>
        //    {
        //        return _ExecuteCommitAction(action, true);
        //    });
        //}

        //#endregion commit with auth

        //#region commit with no auth

        //protected void ExecuteUnauthenticatedCommitAction(Action action)
        //{
        //    _ExecuteCommitAction(action, false);
        //}

        //protected T ExecuteUnauthenticatedCommitAction<T>(Func<T> action)
        //{
        //    return _ExecuteCommitAction(action, false);
        //}

        //protected void ExecuteUnauthenticatedCommitActionTask(Action action)
        //{
        //    Task.Factory.StartNew(() =>
        //    {
        //        _ExecuteCommitAction(action, false);
        //    });
        //}

        //protected Task<T> ExecuteUnauthenticatedCommitActionTask<T>(Func<T> action)
        //{
        //    return Task.Factory.StartNew(() =>
        //    {
        //        return _ExecuteCommitAction(action, false);
        //    });
        //}

        #endregion commit with no auth

        //internal bool _VerifySession(PlaygroundContext context)
        //{
        //    var isAuthenticated = false;

        //    try
        //    {
        //        var sessionToken = _GetSessionToken();

        //        if (!string.IsNullOrEmpty(sessionToken))
        //        {

        //        }

        //        // TODO implement
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }

        //    return isAuthenticated;
        //}

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