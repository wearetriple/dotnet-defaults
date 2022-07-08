using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Buckaroo;

public class DependencyConfiguration
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<ISubscriptionGateway, BuckarooGateway>();

        services.AddTransient<HmacDelegatingHandler>();

        services.AddHttpClient<IPspClient, PspClient>()
            .ConfigureHttpClient(c => c.DefaultRequestHeaders.Add("culture", "nl-NL"))
            .ConfigureHttpClient(c => c.DefaultRequestHeaders.Add("channel", "Web"))
            .AddHttpMessageHandler<HmacDelegatingHandler>();
    }
}
