using Core.Auth;
using Core.Auth.Models.Tenant;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController(ILogger<RegistrationController> logger) : AbstractController(logger)
    {
        [HttpGet]
        [Route("register-tenant")]
        public object RegisterTenant(RegisterTenant_Request parameter)
        {
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                return AUTH.Tenant.CreateOrUpdateTenant(DB_Connection, parameter);
            });
        }
    }
}