using Core.Auth;
using Core.Auth.Models.Account;
using Core.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController(ILogger<AuthenticationController> logger) : AbstractController(logger)
    {
        [HttpPost("login")]
        public ResultOf<LogIn_Response> LogIn(LogIn_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return AUTH.Account.LogIn(HttpContext, DB_Connection, parameter);
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
    }
}