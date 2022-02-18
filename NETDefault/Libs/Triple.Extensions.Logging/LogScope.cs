using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Triple.Extensions.Logging
{
    internal class LogScope : IDisposable
    {
        private readonly ILogger _logger;
        private readonly IDisposable _scope;
        private readonly string _message;
        private readonly object?[] _args;

        public LogScope(ILogger logger, Dictionary<string, object?> data, string message, params object?[] args)
        {
            _logger = logger;
            _message = message;
            _args = args;

            _scope = _logger.BeginScope(data);
            _logger.LogInformation($"{_message} started", _args);
        }

        public void Dispose()
        {
            _logger.LogInformation($"{_message} finished", _args);
            _scope.Dispose();
        }
    }
}
