using FunctionAppNETCore31;
using FunctionAppNETCore31.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;

[assembly: FunctionsStartup(typeof(Startup))]
namespace FunctionAppNETCore31
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            var logLevelSwitch = new LoggingLevelSwitch(LogEventLevel.Warning);
            var url = configuration["Serilog:SeqUrl"];
            var key = configuration["Serilog:SeqKey"];

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(logLevelSwitch)
                .WriteTo.Seq(url, apiKey: key, controlLevelSwitch: logLevelSwitch)
                .CreateLogger();

            builder.Services.AddLogging(builder => builder.AddSerilog(Log.Logger));

            builder.Services.AddOptions<ExampleSettings>().Bind(configuration.GetSection("Example")).ValidateDataAnnotations();
        }
    }
}
