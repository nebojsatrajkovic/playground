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
                var newEntry = new AUTH_Accounts.Model
                {
                    Email = "random@mail.com",
                    IsDeleted = false,
                    Password = "1234"
                };

                newEntry = AUTH_Accounts.DB.Save(DB_Connection, newEntry);

                var debug = AUTH_Accounts.DB.Search(DB_Connection, new AUTH_Accounts.Query
                {
                    IsDeleted = false
                });

                return debug;
            });
        }

        [HttpGet]
        [Route("get-video-stream-by-range")]
        public async Task GetVideoStreamByRange()
        {
            var videoPathFile = Path.Combine("ROOT_FOLDER", "Files", "Videos", "example.mp4");
            var buffer = new byte[1024 * 1024 * 4]; // 'Chunks' of 4MB
            long startPosition = 0;

            if (!string.IsNullOrEmpty(Request.Headers["Range"])) // header:Range value:bytes=0-999
            {
                var range = Request.Headers["Range"].ToString().Split(new char[] { '=', '-' });
                startPosition = long.Parse(range[1]);
            }

            using FileStream inputStream = new(videoPathFile, FileMode.Open, FileAccess.Read, FileShare.Read)
            {
                Position = startPosition
            };

            var chunkSize = await inputStream.ReadAsync(buffer.AsMemory(0, buffer.Length));
            var fileSize = inputStream.Length;

            if (chunkSize > 0)
            {
                Response.StatusCode = 206;
                Response.Headers["Accept-Ranges"] = "bytes";
                Response.Headers["Content-Range"] = $"bytes {startPosition}-{fileSize - 1}/{fileSize}";
                Response.ContentType = "application/octet-stream";

                using Stream outputStream = Response.Body;
                await outputStream.WriteAsync(buffer.AsMemory(0, chunkSize));
            };
        }
    }
}