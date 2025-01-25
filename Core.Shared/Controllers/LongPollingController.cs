using Core.Shared.Utils;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace Core.Shared.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LongPollingController : ControllerBase
    {
        [HttpGet]
        [Route("trigger-request")]
        public async Task<IActionResult> TriggerRequest(string channel, string key, int timeout = 60000)
        {
            var longPollingRequest = new LongPollingRequest(channel, key, timeout);

            var data = await longPollingRequest.WaitAsync();

            return new ObjectResult(new { Data = data });
        }

        public enum LongPolling_Channel
        {
            [Description("ExampleChannel")]
            ExampleChannel
        }

        // NOTE: example usage in some service: LongPollingRequest.Update(LongPolling_Channel.ExampleChannel.Description(), Guid.NewGuid().ToString());

        // builder.Services.AddMvc().AddApplicationPart(typeof(Core.Shared.Controllers.LongPollingController).Assembly);
    }
}