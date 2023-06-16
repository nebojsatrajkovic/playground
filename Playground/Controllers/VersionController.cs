using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace Playground.Controllers
{
    /// <summary>
    /// Version controller
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        private IStringLocalizer<VersionController> _localizer;

        /// <summary>
        /// Default constructor with dependency injection
        /// </summary>
        /// <param name="localizer"></param>
        public VersionController(IStringLocalizer<VersionController> localizer) : base()
        {
            _localizer = localizer;
        }

        /// <summary>
        /// Retrieve version history
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public object Index()
        {
            var values = _localizer.GetAllStrings().OrderByDescending(x => x.Name).ToList();

            return new
            {
                module = Assembly.GetExecutingAssembly().GetName().Name,
                current_version = values.First().Name,
                history = values.Select(x => new { version = x.Name, changes = x.Value.Split(" - ").Where(v => !string.IsNullOrEmpty(v)).ToList() }).ToList()
            };
        }
    }
}