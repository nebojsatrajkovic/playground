using Core.Auth.Controllers.Abstract;
using Core.Cloud.Models.API.File;
using Core.Cloud.Services;
using Core.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudController(ILogger<CloudController> logger) : CORE_AUTH_AbstractController(logger)
    {
        [HttpPost("upload")]
        public async Task<ResultOf<CLOUD_UploadFile_Result>> Upload([FromQuery] int tenantId, [FromQuery] int accountId, [FromQuery] int folderId)
        {
            return await ExecuteCommitAction_Async(async () =>
            {
                return await CloudService.UploadFile(DB_Connection, HttpContext, new CLOUD_UploadFile_Request
                {
                    FolderID = folderId,
                    AccountID = accountId,
                    TenantID = tenantId
                });
            });
        }

        [HttpGet("download/{userAccountId}/{fileId}")]
        public async Task<IActionResult> Download(Guid userAccountID, Guid fileID)
        {
            //var metadata = new FileMetadata(); // TODO load from DB, etc

            //if (metadata == null) return NotFound("File not found.");

            //await Task.Delay(1);

            //var stream = new FileStream(metadata.FilePath, FileMode.Open, FileAccess.Read);

            //return File(stream, "application/octet-stream", metadata.FileName);

            throw new NotImplementedException();
        }
    }
}