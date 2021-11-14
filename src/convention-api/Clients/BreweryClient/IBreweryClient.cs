using System.Collections.Generic;
using System.Threading.Tasks;

namespace convention_api.Clients.BreweryClient
{
    public interface IBreweryClient
    {
        Task<IEnumerable<Brewery>> GetBreweriesByCity(string cityName);
        Task<Brewery> GetBreweryById(string id);
    }
}