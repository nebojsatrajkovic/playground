using Core.DB.Initializers;
using Core.Shared.Configuration;
using Core.Shared.ExceptionHandling;
using CoreCore.DB.Plugin.MSSQL.Configuration;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

#region core settings

var coreConfigurationSection = builder.Configuration.GetSection(nameof(CORE_Configuration));

var coreConfiguration = coreConfigurationSection.Get<CORE_Configuration>();
coreConfiguration.Database = coreConfigurationSection.GetSection(nameof(CORE_Database)).Get<CORE_Database>();

builder.Services.AddSingleton<ICORE_Configuration>(coreConfiguration);

#endregion core settings

#region core generator settings

var coreGeneratorConfiguration = builder.Configuration.GetSection(nameof(CORE_DB_GENERATOR_Configuration)).Get<CORE_DB_GENERATOR_Configuration>();

builder.Services.AddSingleton<ICORE_DB_GENERATOR_Configuration>(coreGeneratorConfiguration);

#endregion core generator settings

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

builder.Services.InitializeCoreDB();

builder.Logging.AddLog4Net();

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