using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetElevation.Core;

namespace NetElevation.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ElevationController : ControllerBase
    {
        private static readonly TileManager tileManager = new TileManager(new TileRepository(new System.IO.DirectoryInfo(@"D:\SRTM\splitted")));

        private readonly ILogger<ElevationController> _logger;

        public ElevationController(ILogger<ElevationController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get(double? latitude, double? longitude)
        {
            if (!latitude.HasValue || !longitude.HasValue)
            {
                return BadRequest("latitude and longitude parameters must be set");
            }

            return Ok(tileManager.GetElevation(latitude.Value, longitude.Value));
        }
    }
}
