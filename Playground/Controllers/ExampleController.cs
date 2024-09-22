using Core.Auth;
using Core.Shared.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController(ILogger<ExampleController> logger) : AbstractController(logger)
    {
        [HttpGet]
        [Route("example")]
        public object Example()
        {
            AUTH.ConfigureConnectionString(CORE_Configuration.Database.ConnectionString);

            //var result = AUTH.CreateOrUpdateTenant(new CreateOrUpdateTenant_Request
            //{
            //    Name = "My remote company"
            //});

            var result = AUTH.CreateOrUpdateAccount();

            return result;
        }
    }
}