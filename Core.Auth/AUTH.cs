using Core.Auth.Controllers;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Auth
{
    public static class AUTH
    {
        public static IMvcBuilder UseCoreAuth(this IMvcBuilder builder)
        {
            builder.AddApplicationPart(typeof(AuthenticationController).Assembly);

            return builder;
        }
    }
}