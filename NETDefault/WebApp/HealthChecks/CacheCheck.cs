using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace WebApp.HealthChecks
{
    // see Health check and application warmup
    public class CacheCheck : IHealthCheck
    {
        private readonly ILogger<CacheCheck> _logger;
        private readonly IMemoryCache _memoryCache;

        public CacheCheck(
            ILogger<CacheCheck> logger,
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue<string>("CacheItem", out _))
            { 
                _logger.LogInformation("Cache check: healthy");
                return Task.FromResult(new HealthCheckResult(HealthStatus.Healthy));
            }
            else
            {
                _logger.LogInformation("Cache check: unhealthy");
                return Task.FromResult(new HealthCheckResult(HealthStatus.Unhealthy));
            }
        }
    }
}
