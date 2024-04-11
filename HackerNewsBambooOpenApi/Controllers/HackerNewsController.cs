using HackerNewsBambooOpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Net;

namespace HackerNewsBambooOpenApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HackerNewsController : ControllerBase
    {
        private static string HackerNewsHttpClientName = "HackerNewsApi";

        private readonly ILogger<HackerNewsController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;

        private readonly MemoryCacheEntryOptions _cacheEntryOptions;

        public HackerNewsController(ILogger<HackerNewsController> logger, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, MemoryCacheEntryOptions cacheEntryOptions)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;

            _cacheEntryOptions = cacheEntryOptions;
        }

        [HttpGet("GetHackerNewsStoriesIds")]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetHackerNewsStoriesIds(CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient(HackerNewsHttpClientName);

                var bestStoriesIds = _memoryCache.Get<List<int>>(Constants.CacheKeyForBestStoriesEntitiesIds);
                if (bestStoriesIds == null)
                {
                    bestStoriesIds = await httpClient.GetFromJsonAsync<List<int>>(new Uri(httpClient.BaseAddress!.ToString() + "/beststories.json"), cancellationToken);
                    _memoryCache.Set(Constants.CacheKeyForBestStoriesEntitiesIds, bestStoriesIds, _cacheEntryOptions);
                }

                if (bestStoriesIds != null && bestStoriesIds!.Count == 0)
                {
                    return NotFound();
                }

                return Ok(bestStoriesIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                _memoryCache.Remove(Constants.CacheKeyForBestStoriesEntitiesIds);

                return BadRequest();
            }
        }

        [HttpGet("GetHackerNewsItemById")]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetHackerNewsItemById(int id, CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient(HackerNewsHttpClientName);

                var newsItem = await httpClient.GetFromJsonAsync<HackerNewsItem>(new Uri(httpClient.BaseAddress!.ToString() + $"/item/{id}.json"), cancellationToken);

                return Ok(newsItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest();
            }
        }

        [HttpGet("GetBestStories")]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ActionResult), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetBestStories(int countOfBestStoriesToReturn, CancellationToken cancellationToken)
        {
            try
            {
                using var httpClient = _httpClientFactory.CreateClient(HackerNewsHttpClientName);

                var cachedBestStoriesIds = _memoryCache.Get<List<int>>(Constants.CacheKeyForBestStoriesEntitiesIds);
                if (cachedBestStoriesIds == null)
                {
                    var bestStoriesIds = await httpClient.GetFromJsonAsync<List<int>>(new Uri(httpClient.BaseAddress!.ToString() + "/beststories.json"), cancellationToken);
                    cachedBestStoriesIds = bestStoriesIds ?? new List<int>();
                    _memoryCache.Set(Constants.CacheKeyForBestStoriesEntitiesIds, cachedBestStoriesIds, _cacheEntryOptions);
                }

                var storiesToRetrieveIds = cachedBestStoriesIds.Take(countOfBestStoriesToReturn);

                List<int> nonExistingItemsIds = new List<int>();

                var cachedHackerNewsItemsIds = _memoryCache.Get<List<HackerNewsItem>>(Constants.CacheKeyForBestStoriesEntities)?.Select(ent => ent.Id).ToList();
                if (cachedHackerNewsItemsIds != null) 
                {
                    nonExistingItemsIds = storiesToRetrieveIds.Where(id => !cachedHackerNewsItemsIds.Contains(id)).ToList();
                }
                else
                {
                    nonExistingItemsIds = storiesToRetrieveIds.ToList();
                }

                var hackerNewsItems = new List<HackerNewsItem>();
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };
                await Parallel.ForEachAsync(nonExistingItemsIds!, parallelOptions, async (id, innerCancellationToken) =>
                {
                    var newsItem = await httpClient.GetFromJsonAsync<HackerNewsItem>(new Uri(httpClient.BaseAddress!.ToString() + $"/item/{id}.json"), innerCancellationToken);

                    hackerNewsItems.Add(newsItem!);
                });

                if (hackerNewsItems.Count > 0)
                {
                    var cachedItems = _memoryCache.Get<List<HackerNewsItem>>(Constants.CacheKeyForBestStoriesEntities)?.ToList();
                    if (cachedItems != null)
                    {
                        cachedItems!.AddRange(hackerNewsItems);

                        _memoryCache.Remove(Constants.CacheKeyForBestStoriesEntities);

                        _memoryCache.Set(Constants.CacheKeyForBestStoriesEntities, cachedItems, _cacheEntryOptions);
                    }
                    else
                    {
                        _memoryCache.Set(Constants.CacheKeyForBestStoriesEntities, hackerNewsItems, _cacheEntryOptions);
                    }
                }

                return Ok(hackerNewsItems.OrderByDescending(ent => ent.Score));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                _memoryCache.Remove(Constants.CacheKeyForBestStoriesEntitiesIds);

                return BadRequest();
            }
        }
    }
}
