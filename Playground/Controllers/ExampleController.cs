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
                //var newEntry = new USR_Accounts.Model
                //{
                //    Email = "random@mail.com"
                //};

                //newEntry = USR_Accounts.DB.Save(DB_Connection, newEntry);

                //var debug = USR_Accounts.DB.Search(DB_Connection, new USR_Accounts.Query
                //{
                //    IsDeleted = false
                //});

                //return debug;

                //var debug = Get_POSTGRE_TEST.Invoke(DB_Connection.Connection, DB_Connection.Transaction, new Get_POSTGRE_TEST.P_POSTGRE_TEST { IDs = new List<long> { 1, 8 } });


                return 0;
            });
        }
    }
}