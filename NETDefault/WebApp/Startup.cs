using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Triple.Extensions.Options;
using WebApp.HealthChecks;
using WebApp.HostedServices;
using WebApp.Models;

namespace WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // see Options pattern
            services.AddOptions<ExampleSettings>().Bind(Configuration.GetSection("Example")).ValidateDataAnnotations().ValidateAtStartupTime();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApp", Version = "v1" });
            });

            services.AddMemoryCache();

            services
                .AddHealthChecks()
                .AddCheck<CacheCheck>("boot");

            // see Health check and application warmup
            // see Hosted service
            services
                .AddHostedService<CacheRenewalService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // see Health check and application warmup
            var httpPaths = Configuration.GetSection("HttpPaths").Get<HttpPathSettings>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApp v1"));
            }

            // see Health check and application warmup
            app.UseWhen(
                httpContext => !httpPaths.Paths.Any(path => httpContext.Request.Path.StartsWithSegments(path)),
                app => app.UseHttpsRedirection());

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // see Health check and application warmup
                endpoints.MapHealthChecks("/apptest", new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        using var streamWriter = new StreamWriter(context.Response.Body);

                        foreach (var entry in report.Entries)
                        {
                            await streamWriter.WriteLineAsync($"{entry.Key}: {entry.Value.Status}.");
                        }

                        await streamWriter.FlushAsync();
                    }
                });
            });
        }
    }
}
