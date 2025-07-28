using Core.Shared.Configuration;
using Core.Shared.ExceptionHandling.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Core.Shared
{
    public static class Initializer
    {
        public static void Initialize_CORE_Configuration(this WebApplicationBuilder builder)
        {
            #region Core configuration

            var section_CoreAPI = builder.Configuration.GetSection(nameof(CORE_API));

            if (section_CoreAPI != null)
            {
                CORE_Configuration.API = section_CoreAPI.Get<CORE_API>() ?? new CORE_API();
            }
            else
            {
                throw new CORE_ConfigurationException("Failed to initialize API configuration!");
            }

            var section_CoreDatabase = builder.Configuration.GetSection(nameof(CORE_Database));

            if (section_CoreAPI != null)
            {
                CORE_Configuration.Database = section_CoreDatabase.Get<CORE_Database>() ?? new CORE_Database();
            }
            else
            {
                throw new CORE_ConfigurationException("Failed to initialize database configuration!");
            }

            #endregion Core configuration

            #region Cloud configuration

            var section_CoreCloud = builder.Configuration.GetSection(nameof(CORE_Cloud));

            if (section_CoreCloud != null)
            {
                CORE_Configuration.Cloud = section_CoreCloud.Get<CORE_Cloud>() ?? new CORE_Cloud();
            }

            #endregion Cloud configuration

            #region Stripe configuration

            var section_Stripe = builder.Configuration.GetSection(nameof(CORE_Stripe));

            if (section_Stripe != null)
            {
                CORE_Configuration.Stripe = section_Stripe.Get<CORE_Stripe>() ?? new CORE_Stripe();
            }

            if (string.IsNullOrEmpty(CORE_Configuration.Stripe?.ApiKey))
            {
                CORE_Configuration.Stripe = new CORE_Stripe
                {
                    ApiKey = Environment.GetEnvironmentVariable($"{nameof(CORE_Stripe)}.{nameof(CORE_Stripe.ApiKey)}") ?? string.Empty
                };
            }

            #endregion Stripe configuration
        }
    }
}