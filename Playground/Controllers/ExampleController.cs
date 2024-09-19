using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController : AbstractController
    {
        public ExampleController(ILogger<ExampleController> logger) : base(logger)
        {

        }

        [HttpGet]
        [Route("example")]
        public object Example()
        {
            //ExecuteUnauthenticatedCommitAction(() => 1);

            //return ExecuteUnauthenticatedCommitAction(() =>
            //{

            //});

            return "OK";
        }
    }
}