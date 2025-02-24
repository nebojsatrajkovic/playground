using Core.Auth.Models.Account;
using Core.Auth.Services;
using Core.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(ILogger<AuthenticationController> logger) : AbstractController(logger)
    {
        [HttpPost("log-in")]
        public ResultOf<LogIn_Response> LogIn(LogIn_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return AuthenticationService.LogIn(HttpContext, DB_Connection, parameter);
            });
        }

        [HttpPost("log-out")]
        public ResultOf LogOut()
        {
            return ExecuteCommitAction(() =>
            {
                return AuthenticationService.LogOut(HttpContext, DB_Connection);
            });
        }

        [HttpPost("check-login-status")]
        public object CheckLoginStatus()
        {
            return ExecuteCommitAction(() =>
            {


                return new ResultOf(CORE_OperationStatus.SUCCESS);
            });
        }

        [HttpPost("trigger-forgot-password")]
        public ResultOf<TriggerForgotPassword_Response> TriggerForgotPassword(TriggerForgotPassword_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return AuthenticationService.TriggerForgotPassword(HttpContext, DB_Connection, parameter);
            });
        }

        [HttpPost("reset-password")]
        public ResultOf<ResetPassword_Response> ResetPassword(ResetPassword_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return AuthenticationService.ResetPassword(DB_Connection, parameter);
            });
        }

        [HttpPost("get-tenants-for-account-email")]
        public ResultOf<List<TenantForAccount>> GetAccountTenants(GetTenantsForAccount_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return AuthenticationService.GetAccountTenants(DB_Connection, parameter);
            });
        }
    }
}