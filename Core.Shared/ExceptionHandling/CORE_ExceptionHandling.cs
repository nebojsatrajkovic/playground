using Core.Shared.ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace Core.Shared.ExceptionHandling
{
    public static class CORE_ExceptionHandling
    {
        public static IApplicationBuilder ConfigureCoreExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();

                    if (contextFeature != null)
                    {
                        if (contextFeature.Error is CORE_UnauthenticatedException)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                            await context.Response.WriteAsync(JsonSerializer.Serialize(new { data = $"{(int)HttpStatusCode.Unauthorized} {HttpStatusCode.Unauthorized} - invalid session" }));
                        }
                    }

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    await context.Response.WriteAsync("Unexpected error has occurred.");
                });
            });

            return app;
        }
    }
}