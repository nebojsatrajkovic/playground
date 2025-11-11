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
            AuthenticateAsync(connection).GetAwaiter().GetResult();
        }

        protected override async Task AuthenticateAsync(CORE_DB_Connection connection)
        {
            var sessionInfo = await AuthenticationService.GetSessionInfo(HttpContext, connection);

            if (!sessionInfo.Succeeded)
            {
                throw new CORE_UnauthenticatedException("Unauthenticated request detected!");
            }

            connection.AccountID = sessionInfo.OperationResult?.AccountID ?? 0;
            connection.TenantID = sessionInfo.OperationResult?.TenantID ?? 0;
            connection.SessionToken = sessionInfo.OperationResult?.SessionToken;
        }

        protected override void Authorize(CORE_DB_Connection connection, List<string> requiredRights)
        {
            AuthorizeAsync(connection, requiredRights).GetAwaiter().GetResult();
        }

        protected override async Task AuthorizeAsync(CORE_DB_Connection connection, List<string> requiredRights)
        {
            var result = await AuthorizationService.ValidateRequiredRightsAsync(connection, requiredRights);

            if (!result.Succeeded)
            {
                throw new CORE_UnauthorizedException("User doesn't have enough rights to perform this action!");
            }
        }
    }
}