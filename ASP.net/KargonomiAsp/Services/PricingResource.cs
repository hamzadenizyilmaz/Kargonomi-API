using System.Globalization;
using Kargonomi.Client.Infrastructure;
using Kargonomi.Client.Models;

namespace Kargonomi.Client;

public sealed class PricingResource
{
    private readonly KargonomiHttpPipeline _pipeline;
    internal PricingResource(KargonomiHttpPipeline pipeline) => _pipeline = pipeline;
    public Task<PriceComparison> CompareAsync(long shipmentId, CancellationToken cancellationToken = default)
    {
        ShipmentsResource.ValidateId(shipmentId);
        return _pipeline.GetAsync<PriceComparison>($"shipment-price-comparison/{shipmentId}", cancellationToken);
    }
    public Task<PriceComparison> ConfirmAsync(long shipmentId, ShippingProviderSelection provider, CancellationToken cancellationToken = default)
    {
        ShipmentsResource.ValidateId(shipmentId);
        if (provider.Id == 0 || provider.Id < -1) throw new ArgumentOutOfRangeException(nameof(provider));
        return _pipeline.SendMultipartAsync<PriceComparison>("confirm-shipping-price", new Dictionary<string, string>
        {
            ["shipment_id"] = shipmentId.ToString(CultureInfo.InvariantCulture),
            ["shipping_provider_id"] = provider.Id.ToString(CultureInfo.InvariantCulture)
        }, cancellationToken);
    }
}
