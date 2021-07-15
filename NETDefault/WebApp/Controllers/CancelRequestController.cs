using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CancelRequestController : ControllerBase
    {
        private readonly ILogger<CancelRequestController> _logger;

        public CancelRequestController(ILogger<CancelRequestController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetAsync(CancellationToken token)
        {
            try
            {
                await Task.Delay(30000, token);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "Client cancelled the request.");

                // since the client is gone, do not do any additional work and return nothing
                return NoContent();
            }

            return "OK";
        }
    }
}
