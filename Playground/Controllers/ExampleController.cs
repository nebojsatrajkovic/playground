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
                var newEntry = new AUTH_Accounts.Model
                {
                    Email = "random@mail.com",
                    IsDeleted = false,
                    Password = "1234"
                };

                var result = AUTH_Accounts.DB.Save(DB_Connection, newEntry);

                var debug = AUTH_Accounts.DB.Search(DB_Connection, new AUTH_Accounts.Query
                {

                });

                return debug;
            });
        }
    }
}