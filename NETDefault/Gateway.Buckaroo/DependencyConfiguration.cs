using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gateway.Buckaroo;

public static class DependencyConfiguration
{
    public static void AddBuckaroo(this IServiceCollection services, HostBuilderContext context)
    {
        services.AddOptions<BuckarooConfiguration>()
                  .Bind(context.Configuration.GetSection(nameof(BuckarooConfiguration)))
                  .ValidateDataAnnotations();

        services.AddScoped<ISubscriptionGateway, BuckarooGateway>();

        services.AddTransient<HmacDelegatingHandler>();

        services.AddHttpClient<IPspClient, PspClient>()
            .ConfigureHttpClient(c => c.DefaultRequestHeaders.Add("culture", "nl-NL"))
            .ConfigureHttpClient(c => c.DefaultRequestHeaders.Add("channel", "Web"))
            .AddHttpMessageHandler<HmacDelegatingHandler>();
    }
}
