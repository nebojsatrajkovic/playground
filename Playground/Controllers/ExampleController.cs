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
            return ExecuteCommitAction(() =>
            {
                var newEntry = new USR_Accounts.Model
                {
                    Email = "random@mail.com"
                };

                newEntry = USR_Accounts.DB.Save(DB_Connection, newEntry);

                var debug = USR_Accounts.DB.Search(DB_Connection, new USR_Accounts.Query
                {
                    IsDeleted = false
                });

                return debug;
            });
        }
    }
}