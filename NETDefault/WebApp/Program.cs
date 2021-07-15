using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Log.Logger.Information("Booting up");

                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                // see Logging with ILogger<> using Serilog to Seq
                Log.Logger.Fatal(ex, "Fatal exception.");
                Log.CloseAndFlush();

                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, logging) =>
                {
                    // see Logging with ILogger<> using Serilog to Seq
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(context.Configuration)
                        .CreateLogger();

                    logging.AddSerilog(Log.Logger);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
