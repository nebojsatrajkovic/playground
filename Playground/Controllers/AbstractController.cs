using Core.DB.Plugin.MSSQL.Controllers;
using Core.Shared.Configuration;
using Core.Shared.ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbstractController : MSSQL_AbstractController
    {
        protected AbstractController(ILogger logger, ICORE_Configuration coreConfiguration) : base(logger, coreConfiguration.Database.ConnectionString)
        {

        }

        protected override void Authenticate()
        {
            bool isAuthenticated = true;

            var sessionToken = GetSessionToken();

            if (!string.IsNullOrEmpty(sessionToken))
            {
                // TODO validate session
            }

            if (!isAuthenticated)
            {
                throw new CORE_UnauthenticatedException("Unauthenticated request detected!");
            }
        }

        protected override string GetSessionToken()
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