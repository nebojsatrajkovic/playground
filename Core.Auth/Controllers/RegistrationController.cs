using Core.Auth.Controllers.Abstract;
using Core.Auth.Models.Account;
using Core.Auth.Models.Tenant;
using Core.Auth.Services;
using Core.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Core.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController(ILogger<RegistrationController> logger) : CORE_AUTH_AbstractController(logger)
    {
        [HttpPost("register-tenant")]
        public ResultOf<RegisterTenant_Response> RegisterTenant(RegisterTenant_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return RegistrationService.RegisterTenant(DB_Connection, parameter);
            });
        }

        [HttpPost("resend-registration-confirmation-email")]
        public ResultOf ResendRegistrationConfirmationEmail(ResendRegistrationConfirmationEmail_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return RegistrationService.ResendRegistrationConfirmationEmail(DB_Connection, parameter);
            });
        }

        [HttpGet("confirm-registration")]
        public ResultOf<ConfirmRegistration_Response> ConfirmRegistration(string token)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return RegistrationService.ConfirmRegistration(DB_Connection, new ConfirmRegistration_Request { Token = token });
            });
        }
    }
}