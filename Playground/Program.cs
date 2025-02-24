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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger()
        .UseSwaggerUI();
}

app.MapControllers();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseForwardedHeaders()
    .ConfigureCoreExceptionHandler();

app.Run();

// TODO method to create or update an account - watch out for the master account
// TODO create account -> decide whether to automatically approve it or he needs to confirm his email address
// TODO method to delete an account
// TODO method to deactivate an account
// TODO method to update tenant data

// TODO implement password validity check - to satisfy security criterias
// TODO implement a way to terminate user's session and update the cache (when it's done via database)