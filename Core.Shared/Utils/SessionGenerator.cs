using Core.Shared.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Core.Shared.Utils
{
    public static class SessionGenerator
    {
        public static void UpdateCookie(HttpContext context, string sessionToken)
        {
            context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            context.Response.Headers.Append(CORE_Configuration.API.AuthKey, sessionToken);

            var domain = context.Request.Host.Host ?? "localhost";

#if DEBUG
            domain = "localhost";
#endif

            context.Response.Cookies.Append(CORE_Configuration.API.AuthKey, sessionToken, new CookieOptions
            {
                Expires = DateTime.Now.AddHours(8),
                Domain = domain,
                IsEssential = true,
                SameSite = SameSiteMode.None,
                Secure = true
            });
        }

        public static string GenerateSessionToken()
        {
            byte[] random = new byte[100];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(random);
            return Convert.ToBase64String(random).Replace(" ", string.Empty).Trim();
        }
    }
}