using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Triple.Extensions.Options
{
    public static class OptionsBuilderExtensions
    {
        /// <summary>
        /// Triggers an OptionsValidationException upon startup when DataAnnotation validation fails.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="builder"></param>
        /// <param name="throwWhenInvalid">Indicates whether the validation should throw during validation (which will abort the application start-up).</param>
        /// <returns></returns>
        public static OptionsBuilder<TOptions> ValidateAtStartupTime<TOptions>(this OptionsBuilder<TOptions> builder, bool throwWhenInvalid = true)
            where TOptions : class
        {
            if (throwWhenInvalid)
            {
                builder.Services.AddTransient<IStartupFilter, ThrowingOptionsValidateFilter<TOptions>>();
            }
            else
            {
                builder.Services.AddTransient<IStartupFilter, OptionsValidateFilter<TOptions>>();
            }
            return builder;
        }

        private class OptionsValidateFilter<TOptions> : IStartupFilter where TOptions : class
        {
            private readonly IOptions<TOptions> _options;
            private readonly ILogger<TOptions> _logger;

            protected bool _throwWhenInvalid;

            public OptionsValidateFilter(IOptions<TOptions> options, ILogger<TOptions> logger)
            {
                _options = options;
                _logger = logger;
            }

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                try
                {
                    _ = _options.Value; // Trigger for validating options.
                }
                catch (OptionsValidationException validationException)
                {
                    _logger.LogError(validationException, "Validation failed of {options}.", $"IOptions<{typeof(TOptions)}>");

                    if (_throwWhenInvalid)
                    {
                        throw;
                    }
                }

                return next;
            }
        }

        private class ThrowingOptionsValidateFilter<TOptions> : OptionsValidateFilter<TOptions> where TOptions : class
        {
            public ThrowingOptionsValidateFilter(IOptions<TOptions> options, ILogger<TOptions> logger) : base(options, logger)
            {
                _throwWhenInvalid = true;
            }
        }
    }
}
