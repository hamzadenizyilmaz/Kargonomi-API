using System.Globalization;
using System.Text.Json.Serialization;
using Kargonomi.Client.Helpers;

namespace Kargonomi.Client.Models;

public sealed record PriceOffer
{
    [JsonPropertyName("id"), JsonConverter(typeof(FlexibleInt64Converter))]
    public long Id { get; init; }
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
    [JsonPropertyName("slug")]
    public string Slug { get; init; } = string.Empty;
    [JsonPropertyName("price")]
    public string? RawPrice { get; init; }
    public bool IsAvailable => !string.Equals(RawPrice, "Hizmet Dışı Bölge", StringComparison.OrdinalIgnoreCase);
    public decimal? ParsedAmount
    {
        get
        {
            var first = RawPrice?.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return decimal.TryParse(first, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ? value : null;
        }
    }
}
public sealed record PriceComparison(
    [property: JsonPropertyName("shipping_provider_with_price")] IReadOnlyList<PriceOffer> Offers,
    [property: JsonPropertyName("shipment")] Shipment Shipment);
public readonly record struct ShippingProviderSelection(long Id)
{
    public static ShippingProviderSelection Automatic { get; } = new(-1);
    public static ShippingProviderSelection Provider(long id) => id > 0
        ? new(id)
        : throw new ArgumentOutOfRangeException(nameof(id), "Provider ID must be positive.");
}
