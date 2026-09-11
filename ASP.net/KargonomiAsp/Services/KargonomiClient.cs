using Kargonomi.Client.Infrastructure;

namespace Kargonomi.Client;

public sealed class KargonomiClient : IDisposable
{
    private readonly KargonomiHttpPipeline _pipeline;
    public KargonomiClient(HttpClient httpClient, KargonomiClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _pipeline = new KargonomiHttpPipeline(httpClient, options);
        Account = new AccountResource(_pipeline);
        Shipments = new ShipmentsResource(_pipeline);
        Pricing = new PricingResource(_pipeline);
        Warehouses = new WarehousesResource(_pipeline);
        Locations = new LocationsResource(_pipeline);
        Barcodes = new BarcodesResource(_pipeline);
        Webhooks = new WebhooksResource(_pipeline);
    }
    public AccountResource Account { get; }
    public ShipmentsResource Shipments { get; }
    public PricingResource Pricing { get; }
    public WarehousesResource Warehouses { get; }
    public LocationsResource Locations { get; }
    public BarcodesResource Barcodes { get; }
    public WebhooksResource Webhooks { get; }
    public void Dispose() => _pipeline.Dispose();
}
