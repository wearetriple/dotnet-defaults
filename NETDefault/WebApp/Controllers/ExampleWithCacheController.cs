using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
    /// <summary>
    /// see Health check and application warmup
    /// </summary>
    [Route("api/example-with-cache")]
    [ApiController]
    public class ExampleWithCacheController : ControllerBase
    {
        private readonly ILogger<ExampleWithCacheController> _logger;
        private readonly IMemoryCache _memoryCache;

        public ExampleWithCacheController(
            ILogger<ExampleWithCacheController> logger,
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public ActionResult<string> GetString()
        {
            var value = _memoryCache.Get<string>("CacheItem");

            if (!string.IsNullOrWhiteSpace(value))
            {
                _logger.LogInformation("Cache: ok");
                return $"{value}";
            }

            _logger.LogInformation("Cache: not-ok");
            return NotFound();
        }

        [HttpGet("check")]
        public async Task<ActionResult> CheckCacheAsync()
        {
            // this endpoints waits until it a certain cache item is available in IMemoryCache
            // because this endpoit waits, it is a good endpoint to use as initializationPage for application warmup
            // be mindful to hide this controller from public use as long-running controller actions can degrade performance
            do
            {
                _logger.LogInformation("Checking cache status..");

                if (_memoryCache.TryGetValue<string>("CacheItem", out var _))
                {
                    _logger.LogInformation("Checking cache status: OK!");
                    return Ok();
                }

                await Task.Delay(1000);
            }
            while (true);
        }
    }
}
