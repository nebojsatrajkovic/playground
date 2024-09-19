using Core.DB.Initializers;
using Core.Shared;
using Core.Shared.ExceptionHandling;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseUrls("http://[::]:21000").UseKestrel(options => { options.Limits.MaxRequestBodySize = long.MaxValue; });

builder.Services.AddControllers().AddJsonOptions(json =>
{
    json.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddCors()
    .AddHttpContextAccessor()
    .AddLocalization(opts => { opts.ResourcesPath = "Resources"; })
    .Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.All;
    });

// NOTE: include controllers from core library
builder.Services.AddMvc().AddApplicationPart(typeof(Core.Shared.Controllers.LongPollingController).Assembly);

builder.Logging.AddLog4Net();

builder.Initialize_CORE_Configuration();

builder.Services.InitializeCoreDB();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseHttpsRedirection()
    .UseForwardedHeaders()
    .ConfigureCoreExceptionHandler();

app.Run();