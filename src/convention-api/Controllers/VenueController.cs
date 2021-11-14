using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using convention_api.Authorization;
using convention_api.Clients.BreweryClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace convention_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VenueController : ControllerBase
    {
        private readonly ILogger<VenueController> _logger;
        private readonly IBreweryClient _breweryClient;

        public VenueController(ILogger<VenueController> logger, IBreweryClient breweryClient)
        {
            _logger = logger;
            _breweryClient = breweryClient;
        }

        [HttpGet("")]
        [Authorize(Policy = PolicyNames.Administration)]
        public async Task<IEnumerable<WeatherForecast>> Get(string city)
        {
            var items = _breweryClient.GetBreweriesByCity(city);
        }

        [HttpGet("secured")]
        [Authorize(Policy = PolicyNames.Administration)]
        public IEnumerable<WeatherForecast> GetSecured()
        {
            return Get();
        }
    }
}