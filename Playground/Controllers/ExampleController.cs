using Core.Shared.Utils;
using CoreDB.Database.DB.Accounts;
using CoreDB.Database.ORM;
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
            ExecuteUnauthenticatedCommitAction(() =>
            {
                var acc = new auth_account.Model
                {
                    auth_account_id = 1,
                    created_at = DateTime.Now,
                    email = "nebojsa.trajkovic92+playground@gmail.com",
                    is_verified = true,
                    password = PasswordHasher.Hash(PasswordGenerator.GenerateRandomPassword(8)),
                    tenant_id = 1,
                    username = "nebojsa.trajkovic92+playground",
                    is_deleted = false,
                    modified_at = DateTime.Now
                };

                auth_account.DB.Save(DB_Connection, acc);

                var dbAcc = Get_Accounts_for_ID.Invoke(DB_Connection.Connection, DB_Connection.Transaction, new P_GAfID { AccountID = 1 });
            });

            return "OK";
        }
    }
}