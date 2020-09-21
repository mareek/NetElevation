using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NetElevation.Core;

namespace NetElevation.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OpenElevationController : ControllerBase
    {
        private readonly ILogger<OpenElevationController> _logger;
        private readonly TileManager _tileManager;
        private readonly bool _disableMultiElevationRequest;

        public OpenElevationController(ILogger<OpenElevationController> logger, TileManager tileManager, IConfiguration config)
        {
            _logger = logger;
            _tileManager = tileManager;
            _disableMultiElevationRequest = config.GetValue<bool>("DisableMultiElevationRequest");
        }

        [HttpGet]
        public IActionResult Get(string locations)
        {
            Location[] parsedLocations;
            try
            {
                parsedLocations = ParseLocations(locations);
            }
            catch
            {
                return BadRequest("request should be like ?locations=10,10|20,20|41.161758,-8.583933");
            }

            if (_disableMultiElevationRequest && parsedLocations.Length > 1)
            {
                return StatusCode(403, "Multiple location per request is disabled on this server");
            }

            _tileManager.SetElevations(parsedLocations);
            return Ok(parsedLocations);
        }

        private Location[] ParseLocations(string locations)
        {
            var result = new Location[locations.Count(c => c == '|') + 1];

            var spanLocations = locations.AsSpan();
            var currentLocationIndex = 0;
            var nextPipeIndex = locations.IndexOf('|');
            var i = 0;
            while (nextPipeIndex >= 0)
            {
                result[i] = ParseLocation(spanLocations[currentLocationIndex..nextPipeIndex]);
                currentLocationIndex = nextPipeIndex + 1;
                nextPipeIndex = locations.IndexOf('|', currentLocationIndex);
                i++;
            }

            result[i] = ParseLocation(spanLocations.Slice(currentLocationIndex));

            return result;
        }

        private static Location ParseLocation(ReadOnlySpan<char> text)
        {
            var commaIndex = text.IndexOf(',');
            var latitude = double.Parse(text.Slice(0, commaIndex), provider: CultureInfo.InvariantCulture);
            var longitude = double.Parse(text.Slice(commaIndex + 1), provider: CultureInfo.InvariantCulture);
            return new Location { Latitude = latitude, Longitude = longitude };
        }
    }
}
