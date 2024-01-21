using Core.DB.Database.Tables;
using Core.Shared.Configuration;
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
            return ExecuteUnauthenticatedCommitAction(() =>
            {
                var newEntry = new AUTH_Accounts.Model
                {

                };

                newEntry = AUTH_Accounts.DB.Save(DB_Connection, newEntry);

                var debug = AUTH_Accounts.DB.Search(DB_Connection, new AUTH_Accounts.Query
                {

                });

                return 0;
            });
        }
    }
}