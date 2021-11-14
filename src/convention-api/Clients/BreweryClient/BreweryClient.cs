using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using MemoryCache = System.Runtime.Caching.MemoryCache;

namespace convention_api.Clients.BreweryClient
{
    /// <summary>
    /// As we are dealing with a lookup api we do not control, we can cache everyhing quite heavily. 
    /// </summary>
    public class CachingBreweryClient : IBreweryClient
    {
        private readonly IBreweryClient _cachedClient;
        private readonly TimeSpan _cacheLength;

        public CachingBreweryClient(IBreweryClient cachedClient, TimeSpan cacheLength)
        {
            _cachedClient = cachedClient;
            _cacheLength = cacheLength;
        }

        public Task<IEnumerable<Brewery>> GetBreweriesByCity(string cityName)
        {
            if (cityName == null) { throw new ArgumentNullException(nameof(cityName)); }

            string cacheKey = nameof(CachingBreweryClient) + nameof(GetBreweriesByCity) +  cityName.ToLower();

            return GetFromCacheOrInsert(cacheKey, 
                () => _cachedClient.GetBreweriesByCity(cityName));
        }

        private async Task<T> GetFromCacheOrInsert<T>(string cacheKey, Func<Task<T>> producer)
        {
            if (MemoryCache.Default.Get(cacheKey) is T cachedObject)
            {
                return cachedObject;
            }

            var element = await producer();

            MemoryCache.Default.Add(cacheKey, 
                element,
                DateTimeOffset.Now + _cacheLength);

            return element;
        }

        public Task<Brewery> GetBreweryById(string id)
        {
            string cacheKey = nameof(CachingBreweryClient) + nameof(GetBreweryById) + id;
            return GetFromCacheOrInsert(cacheKey, () => _cachedClient.GetBreweryById(id));
        }
    }

    /// <summary>
    /// Retrieves breweries. A limited set of functionality compared
    /// to the original API - we do not need more for the demonstrational
    /// purposes used here. 
    /// </summary>
    public class BreweryClient : IBreweryClient
    {
        private readonly Uri _baseUri;

        public BreweryClient(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        public async Task<IEnumerable<Brewery>> GetBreweriesByCity(string cityName)
        {
            var content = await GetContent("/breweries?by_city=" + HttpUtility.UrlEncode(cityName));

            var result = JsonConvert.DeserializeObject<Brewery[]>(content);

            return result;
        }

        private async Task<string> GetContent(string subPath)
        {
            using HttpClient httpClient = new HttpClient();

            var response = await httpClient.GetAsync(new Uri(_baseUri, subPath));

            if (!response.IsSuccessStatusCode)
            {
                throw new ArgumentException("There was a problem connecting to the service. Status code ");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<Brewery> GetBreweryById(string id)
        {
            var body = await GetContent("/breweries/" + HttpUtility.UrlEncode(id));

            return JsonConvert.DeserializeObject<Brewery>(body);
        }
    }
}

