﻿using Core.DB.Plugin.MySQL.Controllers;
using Core.Shared.Configuration;
using Core.Shared.ExceptionHandling.Exceptions;
using CoreCore.DB.Plugin.Shared.Database;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbstractController : MySQL_AbstractController
    {
        protected AbstractController(ILogger logger) : base(logger, CORE_Configuration.Database.ConnectionString)
        {

        }

        protected override void Authenticate(CORE_DB_Connection dbConnection)
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