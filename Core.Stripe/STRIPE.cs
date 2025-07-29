using Core.Shared.Configuration;
using Core.Stripe.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Core.Stripe
{
    public static class STRIPE
    {
        public static IMvcBuilder UseStripeIntegration(this IMvcBuilder builder)
        {
            builder.AddApplicationPart(typeof(StripeController).Assembly);

            StripeConfiguration.ApiKey = CORE_Configuration.Stripe.ApiKey;

            return builder;
        }
    }
}