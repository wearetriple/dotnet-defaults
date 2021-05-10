using System;
using FunctionAppNET50.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FunctionAppNET50
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                CreateHostBuilder().Build().Run();
            }
            catch (Exception ex)
            {
                // see Logging with ILogger<> using Serilog to Seq
                Log.Logger.Fatal(ex, "Fatal exception.");
                Log.CloseAndFlush();

                throw;
            }
        }

        private static IHostBuilder CreateHostBuilder() => 
            new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureLogging((context, logging) =>
                {
                    // see Logging with ILogger<> using Serilog to Seq
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(context.Configuration)
                        .CreateLogger();

                    logging.AddSerilog(Log.Logger);
                })
                .ConfigureServices((context, services) =>
                {
                    // see Options pattern
                    services.AddOptions<ExampleSettings>().Bind(context.Configuration.GetSection("Example")).ValidateDataAnnotations();
                });
    }
}
