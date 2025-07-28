using Core.Shared.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Core.Stripe
{
    public static class STRIPE
    {
        public static IMvcBuilder UseStripeIntegration(this IMvcBuilder builder)
        {
            StripeConfiguration.ApiKey = CORE_Configuration.Stripe.ApiKey;

            return builder;
        }
    }
}