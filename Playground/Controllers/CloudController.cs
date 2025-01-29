using Core.Cloud.Models;
using Core.Cloud.Services;
using Core.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Playground.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudController : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<ResultOf<FileMetadata>> Upload([FromQuery] Guid userAccountId, [FromQuery] Guid folderId)
        {
            return await CloudService.UploadFile(HttpContext, folderId, userAccountId);
        }

        [HttpGet("download/{userAccountId}/{fileId}")]
        public async Task<IActionResult> Download(Guid userAccountID, Guid fileID)
        {
            var metadata = new FileMetadata(); // TODO load from DB, etc

            if (metadata == null) return NotFound("File not found.");

            await Task.Delay(1);

            var stream = new FileStream(metadata.FilePath, FileMode.Open, FileAccess.Read);

            return File(stream, "application/octet-stream", metadata.FileName);
        }
    }
}