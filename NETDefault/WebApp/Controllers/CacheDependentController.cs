using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
    /// <summary>
    /// see Health check and application warmup
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CacheDependentController : ControllerBase
    {
        private readonly ILogger<CacheDependentController> _logger;
        private readonly IMemoryCache _memoryCache;

        public CacheDependentController(
            ILogger<CacheDependentController> logger,
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
