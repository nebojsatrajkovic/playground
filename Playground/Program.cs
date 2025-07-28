using Core.Auth;
using Core.Shared;
using Core.Shared.ExceptionHandling;
using Core.Stripe;
using Core.Stripe.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateSlimBuilder(args);
builder.WebHost.UseUrls("http://[::]:21000").UseKestrel(options => { options.Limits.MaxRequestBodySize = long.MaxValue; });

builder.Initialize_CORE_Configuration();

builder.Services.AddControllers()
    .AddJsonOptions(json =>
    {
        json.JsonSerializerOptions.PropertyNamingPolicy = null;
    })
    .UseCoreAuth()
    .UseStripeIntegration();

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

string? productId = null;

var product = await StripeService.CreateOrUpdateProduct(new Core.Stripe.Models.Product.STRIPE_CreateOrUpdateProduct_Request
{
    ProductId = productId,
    ProductName = "Basic package",
    ProductDescription = "Basic",
    PriceData = new Core.Stripe.Models.Product.STRIPE_COUP_R_PriceInfo
    {
        Price = 10,
        IsRecurring = true,
        IfRecurring_IsWeekly = true,
        IfRecurring_IntervalCount = 1
    }
});

product = await StripeService.CreateOrUpdateProduct(new Core.Stripe.Models.Product.STRIPE_CreateOrUpdateProduct_Request
{
    ProductId = productId,
    ProductName = "Standard package",
    ProductDescription = "Standard",
    PriceData = new Core.Stripe.Models.Product.STRIPE_COUP_R_PriceInfo
    {
        Price = 17,
        IsRecurring = true,
        IfRecurring_IsMonthly = true,
        IfRecurring_IntervalCount = 1
    }
});

product = await StripeService.CreateOrUpdateProduct(new Core.Stripe.Models.Product.STRIPE_CreateOrUpdateProduct_Request
{
    ProductId = productId,
    ProductName = "Premium package",
    ProductDescription = "Premium",
    PriceData = new Core.Stripe.Models.Product.STRIPE_COUP_R_PriceInfo
    {
        Price = 30,
        IsRecurring = true,
        IfRecurring_IsYearly = true,
        IfRecurring_IntervalCount = 1
    }
});

app.Run();