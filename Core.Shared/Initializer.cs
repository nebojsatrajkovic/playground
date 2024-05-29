using Core.Shared.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Shared
{
    public static class Initializer
    {
        public static void Initialize_CORE_Configuration(this WebApplicationBuilder builder)
        {
            var coreConfigurationSection = builder.Configuration.GetSection(nameof(CORE_Configuration));

            if (coreConfigurationSection != null)
            {
                var coreConfiguration = coreConfigurationSection.Get<CORE_Configuration>() ?? new CORE_Configuration();
                var databaseConfigurationSection = coreConfigurationSection.GetSection(nameof(CORE_Database));
                coreConfiguration.Database = databaseConfigurationSection.Get<CORE_Database>() ?? new CORE_Database();

                builder.Services.AddSingleton<ICORE_Configuration>(coreConfiguration);
            }
            else
            {
                throw new Exception("Failed to initialize core configuration!");
            }
        }
    }
}