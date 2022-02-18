using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Triple.Extensions.Logging
{
    internal class LogScopeBuilder : ILogScopeBuilder
    {
        private readonly Dictionary<string, object?> _data = new();
        private readonly ILogger _logger;

        public LogScopeBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public ILogScopeBuilder AddToScope<TValue>(TValue value, [CallerArgumentExpression("value")] string argumentExpression = "")
        {
            var property = argumentExpression.Split(".")[^1].ToFirstLetterLower();

            _data[property] = value;

            return this;
        }

        public IDisposable BeginScope() => _logger.BeginScope(_data);

        public IDisposable BeginScope(string message, params object?[] args) => new LogScope(_logger, _data, message, args);
    }
}
