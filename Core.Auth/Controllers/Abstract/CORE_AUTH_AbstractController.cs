using Core.Auth.Services;
using Core.DB.Plugin.MySQL.Controllers;
using Core.Shared.Configuration;
using Core.Shared.ExceptionHandling.Exceptions;
using CoreCore.DB.Plugin.Shared.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Core.Auth.Controllers.Abstract
{
    [Route("api/[controller]")]
    [ApiController]
    public class CORE_AUTH_AbstractController : MySQL_ResultOf_AbstractController
    {
        protected CORE_AUTH_AbstractController(ILogger logger) : base(logger, CORE_Configuration.Database.ConnectionString)
        {

        }

        protected override void Authenticate(CORE_DB_Connection connection)
        {
            var sessionInfo = AuthenticationService.GetSessionInfo(HttpContext, connection);

            if (!sessionInfo.Succeeded)
            {
                throw new CORE_UnauthenticatedException("Unauthenticated request detected!");
            }

            connection.AccountID = sessionInfo.OperationResult?.AccountID ?? 0;
            connection.TenantID = sessionInfo.OperationResult?.TenantID ?? 0;
        }
    }
}