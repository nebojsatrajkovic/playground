using Core.DB.Database;
using Core.Shared.Configuration;
using Core.Shared.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController : AbstractController
    {
        public ExampleController(ILogger<ExampleController> logger, ICORE_Configuration coreConfiguration) : base(logger, coreConfiguration)
        {

        }

        [HttpPost]
        [Route("example")]
        public object Example()
        {
            return ExecuteCommitAction(() =>
            {
                var debug = AUTH_Accounts.DB.Search(DB_Connection, new AUTH_Accounts.Query
                {

                });

                return debug;
            });
        }
    }
}