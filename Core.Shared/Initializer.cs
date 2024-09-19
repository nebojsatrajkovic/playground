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
        }
    }
}