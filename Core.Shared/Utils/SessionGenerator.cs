using Core.Shared.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;

namespace Core.Shared.Utils
{
    public static class SessionGenerator
    {
        public static void UpdateCookie(IHttpContextAccessor httpContextAccessor, string sessionToken)
        {
            httpContextAccessor.HttpContext?.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            httpContextAccessor.HttpContext?.Response.Headers.Append(CORE_Configuration.AuthKey, sessionToken);

            var domain = httpContextAccessor.HttpContext?.Request.Host.Host ?? "localhost";

#if DEBUG
            domain = "localhost";
#endif

            httpContextAccessor.HttpContext?.Response.Cookies.Append(CORE_Configuration.AuthKey, sessionToken, new CookieOptions
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