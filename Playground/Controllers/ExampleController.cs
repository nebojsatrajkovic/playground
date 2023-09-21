using Core.Shared.Configuration;
using Core.Shared.Controllers;
using Core.Shared.Database.Generator;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController : AbstractController
    {
        readonly ICORE_DB_GENERATOR_Configuration coreGeneratorConfiguration;

        public ExampleController(ILogger<ExampleController> logger, ICORE_Configuration coreConfiguration, ICORE_DB_GENERATOR_Configuration coreGeneratorConfiguration) : base(logger, coreConfiguration)
        {
            this.coreGeneratorConfiguration = coreGeneratorConfiguration;
        }

        [HttpPost]
        [Route("example")]
        public object Example()
        {
            return ExecuteCommitAction(() =>
            {
                CORE_MSSQL_DB_Generator.GenerateORMs_FromMSSQL(DB_Connection, coreGeneratorConfiguration);

                //var newEntry = new AUTH_Accounts.Model
                //{
                //    Email = "random@mail.com",
                //    IsDeleted = false,
                //    Password = "1234"
                //};

                //var result = AUTH_Accounts.DB.Save(DB_Connection, newEntry);

                //var debug = AUTH_Accounts.DB.Search(DB_Connection, new AUTH_Accounts.Query
                //{

                //});

                return 0;
            });
        }
    }
}