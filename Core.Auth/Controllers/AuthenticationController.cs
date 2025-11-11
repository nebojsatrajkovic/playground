using Core.Auth.Controllers.Abstract;
using Core.Auth.Models.Account;
using Core.Auth.Services;
using Core.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Core.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(ILogger<AuthenticationController> logger) : CORE_AUTH_AbstractController(logger)
    {
        [HttpPost("log-in")]
        public async Task<ResultOf<LogIn_Response>> LogIn(LogIn_Request parameter)
        {
            return await ExecuteUnauthenticatedCommitAction_Async(async () =>
            {
                return await AuthenticationService.LogIn(HttpContext, DB_Connection, parameter);
            });
        }

        [HttpPost("log-out")]
        public async Task<ResultOf> LogOut()
        {
            return await ExecuteCommitAction_Async(async () =>
            {
                return await AuthenticationService.LogOut(HttpContext, DB_Connection);
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
        public async Task<ResultOf<TriggerForgotPassword_Response>> TriggerForgotPassword(TriggerForgotPassword_Request parameter)
        {
            return await ExecuteUnauthenticatedCommitAction_Async(async () =>
            {
                return await AuthenticationService.TriggerForgotPassword(HttpContext, DB_Connection, parameter);
            });
        }

        [HttpPost("reset-password")]
        public async Task<ResultOf<ResetPassword_Response>> ResetPassword(ResetPassword_Request parameter)
        {
            return await ExecuteUnauthenticatedCommitAction_Async(async () =>
            {
                return await AuthenticationService.ResetPassword(DB_Connection, parameter);
            });
        }

        [HttpPost("get-tenants-for-account-email")]
        public async Task<ResultOf<List<TenantForAccount>>> GetAccountTenants(GetTenantsForAccount_Request parameter)
        {
            return await ExecuteUnauthenticatedCommitAction_Async(async () =>
            {
                return await AuthenticationService.GetAccountTenants(DB_Connection, parameter);
            });
        }
    }
}