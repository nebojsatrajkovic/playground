using Core.Auth;
using Core.Auth.Models.Account;
using Core.Auth.Models.Tenant;
using Core.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController(ILogger<RegistrationController> logger) : AbstractController(logger)
    {
        [HttpGet("register-tenant")]
        public ResultOf<RegisterTenant_Response> RegisterTenant(RegisterTenant_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return AUTH.Tenant.CreateOrUpdateTenant(DB_Connection, parameter);
            });
        }

        [HttpGet("confirm-registration")]
        public ResultOf<ConfirmRegistration_Response> ConfirmRegistration(string token)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return AUTH.Account.ConfirmRegistration(DB_Connection, new ConfirmRegistration_Request { Token = token });
            });
        }

        [HttpPost("resend-registration-confirmation-email")]
        public ResultOf ResendRegistrationConfirmationEmail(ResendRegistrationConfirmationEmail_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return AUTH.Account.ResendRegistrationConfirmationEmail(DB_Connection, parameter);
            });
        }
    }
}