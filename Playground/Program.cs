using Core.Auth;
using Core.Shared;
using Core.Shared.ExceptionHandling;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseUrls("http://[::]:21000").UseKestrel(options => { options.Limits.MaxRequestBodySize = long.MaxValue; });

builder.Initialize_CORE_Configuration();

builder.Services.AddControllers()
    .AddJsonOptions(json =>
    {
        json.JsonSerializerOptions.PropertyNamingPolicy = null;
    })
    .UseCoreAuth();

builder.Services.AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddCors()
    .AddHttpContextAccessor()
    .AddLocalization(opts => { opts.ResourcesPath = "Resources"; })
    .Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.All;
    });

builder.Logging.AddLog4Net();

var app = builder.Build();

app.UseRouting()
    .UseEndpoints(endpoints => { endpoints.MapControllers(); })
    .UseCors(x =>
    {
        x.AllowAnyMethod()
        .AllowAnyHeader()
        .SetIsOriginAllowed(origin => true)
        .AllowCredentials();
    })
    .UseForwardedHeaders()
    .ConfigureCoreExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger().UseSwaggerUI();
}

app.Run();