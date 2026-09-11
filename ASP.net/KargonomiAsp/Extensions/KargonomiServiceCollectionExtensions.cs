using Kargonomi.Client;
using Microsoft.Extensions.Http.Resilience;

namespace Microsoft.Extensions.DependencyInjection;

public static class KargonomiServiceCollectionExtensions
{
    public const string HttpClientName = "Kargonomi";
    public static IServiceCollection AddKargonomi(this IServiceCollection services, Action<KargonomiServiceOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var configured = new KargonomiServiceOptions();
        configure(configured);
        var clientOptions = configured.ToClientOptions();

        services.AddHttpClient(HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false })
            .AddStandardResilienceHandler(options => options.Retry.DisableForUnsafeHttpMethods());
        services.AddTransient(serviceProvider =>
            new KargonomiClient(serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName), clientOptions));
        return services;
    }
}
