using Core.DB.Initializers;
using Core.Shared;
using Core.Shared.ExceptionHandling;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(json =>
{
    json.JsonSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

builder.Services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.AddHttpContextAccessor();

// NOTE: include controllers from core library
builder.Services.AddMvc().AddApplicationPart(typeof(Core.Shared.Controllers.LongPollingController).Assembly);

builder.Logging.AddLog4Net();

builder.Initialize_CORE_Configuration();

builder.Services.InitializeCoreDB();

var app = builder.Build();

var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseHttpsRedirection();

app.UseForwardedHeaders();

app.UseAuthorization();

app.MapControllers();

app.ConfigureCustomExceptionHandler();

app.Run();