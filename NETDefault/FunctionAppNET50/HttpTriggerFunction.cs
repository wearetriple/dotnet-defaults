using System.Net;
using FunctionAppNET50.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FunctionAppNET50
{
    public class HttpTriggerFunction
    {
        private readonly ILogger<HttpTriggerFunction> _logger;
        private readonly ExampleSettings _settings;

        public HttpTriggerFunction(ILogger<HttpTriggerFunction> logger, IOptions<ExampleSettings> options)
        {
            _logger = logger;
            _settings = options.Value;
        }

        [Function("HttpTriggerFunction")]
        public HttpResponseData Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            _logger.LogWarning("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
