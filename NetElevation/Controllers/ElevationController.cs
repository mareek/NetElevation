using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetElevation.Core;

namespace NetElevation.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ElevationController : ControllerBase
    {
        private readonly ILogger<ElevationController> _logger;
        private readonly TileManager _tileManager;
        private readonly bool _disableMultiElevationRequest;

        public ElevationController(ILogger<ElevationController> logger, TileManager tileManager, IConfiguration config)
        {
            _logger = logger;
            _tileManager = tileManager;
            _disableMultiElevationRequest = config.GetValue<bool>("DisableMultiElevationRequest");
        }

        [HttpGet]
        public IActionResult Get(double? latitude, double? longitude)
        {
            if (!latitude.HasValue || !longitude.HasValue)
            {
                return BadRequest("latitude and longitude parameters must be set");
            }

            return Ok(_tileManager.GetElevation(latitude.Value, longitude.Value));
        }

        [HttpPost]
        public IActionResult Post(Location[] locations)
        {
            if (_disableMultiElevationRequest)
            {
                return StatusCode(405, "Post method is disabled on this server");
            }

            _tileManager.SetElevations(locations);
            return Ok(locations);
        }
    }
}
