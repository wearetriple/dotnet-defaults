using System;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Triple.Extensions.Logging
{
    public static class ILoggerExtensions
    {
        public static IDisposable BeginHttpContextScope(this ILogger logger, HttpContext httpContext)
        {
            var builder = logger.AddToScope(httpContext.TraceIdentifier);

            // Log the X-Trace request header from the request to trace remote requests.
            if (httpContext.Request.Headers.TryGetValue("X-Trace", out var headerValues) &&
                headerValues.ToString() is string remoteTraceIdentifier)
            {
                builder.AddToScope(remoteTraceIdentifier);
            }

            return builder.BeginScope("Request");
        }

        public static ILogScopeBuilder AddToScope<TValue>(this ILogger logger, TValue value, [CallerArgumentExpression("value")] string argumentExpression = "")
        {
            return new LogScopeBuilder(logger).AddToScope(value, argumentExpression);
        }
    }
}
