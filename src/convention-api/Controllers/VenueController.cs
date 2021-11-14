using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using convention_api.Authorization;
using convention_api.Clients.BreweryClient;
using convention_api.Model;
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
        public async Task<IEnumerable<Venue>> Get(string city)
        {
            var items = await _breweryClient.GetBreweriesByCity(city);

            return items
                .Where(x => x.BreweryType != BreweryType.Closed
                            && x.BreweryType != BreweryType.Planning
                            && x.BreweryType != BreweryType.Proprietor
                            && x.BreweryType != BreweryType.None)
                .Select(Venue.From);
        }

        [HttpGet("bycity")]
        public async Task<IEnumerable<Venue>> GetByCity(string city)
        {
            var items = await _breweryClient.GetBreweriesByCity(city);

            return items
                //.Where(x => x.BreweryType != BreweryType.Closed
                //            && x.BreweryType != BreweryType.Planning
                //            && x.BreweryType != BreweryType.Proprietor
                //            && x.BreweryType != BreweryType.None)
                .Select(Venue.From);
        }
    }
}