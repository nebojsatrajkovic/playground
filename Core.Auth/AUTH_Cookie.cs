using Core.Shared.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Core.Auth
{
    internal static class AUTH_Cookie
    {
        internal static void UpdateCookie(HttpContext context, string sessionToken)
        {
            context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            context.Response.Headers.Append(CORE_Configuration.API.AuthKey, sessionToken);

            var domain = context.Request.Host.Host ?? "localhost";

#if DEBUG
            domain = "localhost";
#endif

            context.Response.Cookies.Append(CORE_Configuration.API.AuthKey, sessionToken, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddHours(8),
                Domain = domain,
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });
        }

        internal static string GenerateSessionToken()
        {
            byte[] random = new byte[100];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(random);
            return Convert.ToBase64String(random).Replace(" ", string.Empty).Trim();
        }

        internal static string GetSessionToken(HttpContext context)
        {
            var sessionToken = string.Empty;

            try
            {
                context.Request.Cookies.TryGetValue(CORE_Configuration.API.AuthKey, out sessionToken);

                if (string.IsNullOrEmpty(sessionToken))
                {
                    context.Request.Headers.TryGetValue(CORE_Configuration.API.AuthKey, out var sessionTokenValue);

                    sessionToken = sessionTokenValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return sessionToken ?? string.Empty;
        }

        internal static void RemoveCookie(HttpContext context)
        {
            var domain = context.Request.Host.Host ?? "localhost";

#if DEBUG
            domain = "localhost";
#endif

            context.Response.Cookies.Append(CORE_Configuration.API.AuthKey, string.Empty, new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                Domain = domain,
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });
        }
    }
}