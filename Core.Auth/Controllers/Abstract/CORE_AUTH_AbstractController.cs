using Core.Auth.Models.Account;
using Core.Auth.Services;
using Core.DB.Plugin.MySQL.Controllers;
using Core.Shared.Configuration;
using Core.Shared.ExceptionHandling.Exceptions;
using CoreCore.DB.Plugin.Shared.Database;
using Microsoft.AspNetCore.Http;
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

        protected override void Authenticate(CORE_DB_Connection dbConnection)
        {
            bool isAuthenticated = false;

            var sessionToken = GetSessionToken();

            if (!string.IsNullOrEmpty(sessionToken))
            {
                var validationResult = AuthenticationService.ValidateSession(HttpContext, dbConnection, new ValidateSession_Request { SessionToken = sessionToken });

                isAuthenticated = validationResult.Succeeded;
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
                HttpContext.Request.Cookies.TryGetValue(CORE_Configuration.API.AuthKey, out sessionToken);

                if (string.IsNullOrEmpty(sessionToken))
                {
                    HttpContext.Request.Headers.TryGetValue(CORE_Configuration.API.AuthKey, out var sessionTokenValue);

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