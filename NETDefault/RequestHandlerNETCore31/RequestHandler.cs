using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RequestHandlerNETCore31.HelperClasses;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace RequestHandlerNETCore31
{
    public static class RequestHandler
    {
        /// <summary>
        /// Invokes requestHandler and catches common exceptions.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="logger"></param>
        /// <param name="requestHandler"></param>
        /// <param name="functionMethodName">Gets filled automatically by compiler</param>
        /// <returns></returns>
        public static Task<IActionResult> HandleAsync(HttpRequest request, ILogger logger, Func<Task<IActionResult>> requestHandler, [CallerMemberName] string functionMethodName = "")
            => HandleResponseAsync(request, logger, requestHandler, functionMethodName);

        /// <summary>
        /// Invokes requestHandler and catches common exceptions.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="logger"></param>
        /// <param name="requestHandler"></param>
        /// <param name="functionMethodName">Gets filled automatically by compiler</param>
        /// <returns></returns>
        public static Task<IActionResult> Handle(HttpRequest request, ILogger logger, Func<IActionResult> requestHandler, [CallerMemberName] string functionMethodName = "")
            => HandleResponseAsync(request, logger, () => Task.FromResult(requestHandler()), functionMethodName);

        /// <summary>
        /// Validates TRequestModel and invokes requestHandler.
        /// 
        /// Catches common exceptions.
        /// </summary>
        /// <typeparam name="TRequestModel"></typeparam>
        /// <param name="request"></param>
        /// <param name="logger"></param>
        /// <param name="requestHandler"></param>
        /// /// <param name="functionMethodName">Gets filled automatically by compiler</param>
        /// <returns></returns>
        public static async Task<IActionResult> HandleAsync<TRequestModel>(HttpRequest request, ILogger logger, Func<TRequestModel, Task<IActionResult>> requestHandler, [CallerMemberName] string functionMethodName = "")
        {
            var body = await request.ReadAsStringAsync();
            var requestBody = JsonConvert.DeserializeObject<TRequestModel>(body);

            return request is null
                ? new BadRequestResult()
                : await HandleValidationAsync(request, requestBody, logger, requestHandler, functionMethodName);
        }

        private static async Task<IActionResult> HandleValidationAsync<TRequestModel>(HttpRequest request, TRequestModel requestBody, ILogger logger, Func<TRequestModel, Task<IActionResult>> requestHandler, string functionMethodName)
        {
            var validationResult = Utilities.ModelValidator(requestBody);
            
            return !validationResult.IsValid
                ? new BadRequestObjectResult(validationResult.ValidationResults)
                : await HandleResponseAsync(request, logger, () => requestHandler(requestBody), functionMethodName);
        }

        private static async Task<IActionResult> HandleResponseAsync(HttpRequest request, ILogger logger, Func<Task<IActionResult>> requestHandler, string functionMethodName)
        {
            try
            {
                var response = await requestHandler();
                return response;
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning(ex, "{functionName}: Invalid credentials", functionMethodName, request.HttpContext.TraceIdentifier);

                return new StatusCodeResult(401);

                // return new ObjectResult("Something to return") { StatusCode = Status401Unauthorized };
            }
            catch (NotFoundException ex)
            {
                logger.LogError(ex, "{functionName}: Not found", functionMethodName);

                return new StatusCodeResult(404);

                // return new ObjectResult("Something to return") { StatusCode = Status404NotFound };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{functionName}: Unhandled exception", functionMethodName);

                return new StatusCodeResult(500);

                // return new ObjectResult("Something to return") { StatusCode = Status500InternalServerError };
            }
        }
    }
}
