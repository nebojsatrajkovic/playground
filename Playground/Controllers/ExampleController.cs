using Core.Auth;
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
            //var result = AUTH.CreateOrUpdateTenant(new CreateOrUpdateTenant_Request
            //{
            //    Name = "My remote company"
            //});

            var result = AUTH.Account.CreateOrUpdateAccount();

            return result;
        }
    }
}