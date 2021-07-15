using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApp.HostedServices
{
    // see Health check and application warmup
    // this CacheRenewalService emulates a slow application start-up by making the cache available 30 seconds after startup.
    public class CacheRenewalService : BackgroundService
    {
        private readonly ILogger<CacheRenewalService> _logger;
        private readonly IMemoryCache _memoryCache;

        public CacheRenewalService(
            ILogger<CacheRenewalService> logger,
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Renewing cache..");
            try
            {
                do
                {
                    await Task.Delay(30000, stoppingToken);

                    _logger.LogInformation("Cache renewed!");

                    _memoryCache.Set("CacheItem", $"{Guid.NewGuid()}: String from cache!");
                }
                while (!stoppingToken.IsCancellationRequested);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Cound not finish cache renewal");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during cache renewal");
            }
        }
    }
}
