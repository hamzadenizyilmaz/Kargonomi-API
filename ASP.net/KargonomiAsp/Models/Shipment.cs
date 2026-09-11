using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kargonomi.Client.Models;

public sealed record Shipment
{
    [JsonPropertyName("id")]
    public long Id { get; init; }
    [JsonPropertyName("type")]
    public string Type { get; init; } = string.Empty;
    [JsonPropertyName("shipping_webservice_order_id")]
    public string? ShippingWebserviceOrderId { get; init; }
    [JsonPropertyName("shipping_webservice_barcode")]
    public string? ShippingWebserviceBarcode { get; init; }
    [JsonPropertyName("shipping_webservice_tracking_code")]
    public string? ShippingWebserviceTrackingCode { get; init; }
    [JsonPropertyName("shipping_provider_name")]
    public string? ShippingProviderName { get; init; }
    [JsonPropertyName("shipping_provider_slug")]
    public string? ShippingProviderSlug { get; init; }
    [JsonPropertyName("barcode_of_order_id")]
    public string? BarcodeOfOrderId { get; init; }
    [JsonPropertyName("status")]
    public ShipmentStatus Status { get; init; }
    [JsonPropertyName("status_label")]
    public string StatusLabel { get; init; } = string.Empty;
    [JsonPropertyName("shipping_webservice_created_at")]
    public DateTimeOffset? ShippingWebserviceCreatedAt { get; init; }
    [JsonPropertyName("ecommerce_provider_order_no")]
    public string? EcommerceProviderOrderNo { get; init; }
    [JsonPropertyName("ecommerce_provider")]
    public string? EcommerceProvider { get; init; }
    [JsonPropertyName("package_count")]
    public int PackageCount { get; init; }
    [JsonPropertyName("estimated_price")]
    public JsonElement? EstimatedPrice { get; init; }
    [JsonPropertyName("real_price")]
    public JsonElement? RealPrice { get; init; }
    [JsonPropertyName("extra_shipping_price")]
    public string? ExtraShippingPrice { get; init; }
    [JsonPropertyName("buyer_name")]
    public string? BuyerName { get; init; }
    [JsonPropertyName("delivery_date_to_shipment_office")]
    public DateTimeOffset? DeliveryDateToShipmentOffice { get; init; }
    [JsonPropertyName("shipping_provider_customer_delivery_date")]
    public DateTimeOffset? ShippingProviderCustomerDeliveryDate { get; init; }
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }
    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }
    [JsonPropertyName("sender")]
    public ShipmentParty? Sender { get; init; }
    [JsonPropertyName("buyer")]
    public ShipmentParty? Buyer { get; init; }
    [JsonPropertyName("pricing")]
    public ShipmentPricing? Pricing { get; init; }
    [JsonPropertyName("warehouse")]
    public Warehouse? Warehouse { get; init; }
    [JsonPropertyName("shipment_packages")]
    public IReadOnlyList<ShipmentPackage> Packages { get; init; } = [];
    [JsonExtensionData]
    public IDictionary<string, JsonElement> AdditionalData { get; init; } = new Dictionary<string, JsonElement>();
}
public sealed record ShipmentParty(
    [property: JsonPropertyName("sender_name")] string? SenderName,
    [property: JsonPropertyName("buyer_name")] string? BuyerName,
    [property: JsonPropertyName("sender_email")] string? SenderEmail,
    [property: JsonPropertyName("buyer_email")] string? BuyerEmail,
    [property: JsonPropertyName("sender_phone")] string? SenderPhone,
    [property: JsonPropertyName("buyer_phone")] string? BuyerPhone,
    [property: JsonPropertyName("sender_phone2")] string? SenderPhone2,
    [property: JsonPropertyName("buyer_phone2")] string? BuyerPhone2,
    [property: JsonPropertyName("sender_tax_number")] string? SenderTaxNumber,
    [property: JsonPropertyName("buyer_tax_number")] string? BuyerTaxNumber,
    [property: JsonPropertyName("sender_tax_place")] string? SenderTaxPlace,
    [property: JsonPropertyName("buyer_tax_place")] string? BuyerTaxPlace,
    [property: JsonPropertyName("sender_address")] string? SenderAddress,
    [property: JsonPropertyName("buyer_address")] string? BuyerAddress,
    [property: JsonPropertyName("sender_state")] string? SenderState,
    [property: JsonPropertyName("buyer_state")] string? BuyerState,
    [property: JsonPropertyName("sender_city")] string? SenderCity,
    [property: JsonPropertyName("buyer_city")] string? BuyerCity);
public sealed record ShipmentPricing(
    [property: JsonPropertyName("package_count")] int PackageCount,
    [property: JsonPropertyName("estimated_price")] JsonElement? EstimatedPrice,
    [property: JsonPropertyName("real_price")] JsonElement? RealPrice,
    [property: JsonPropertyName("extra_shipping_price")] string? ExtraShippingPrice,
    [property: JsonPropertyName("price_diff")] JsonElement? PriceDifference);
public sealed record Warehouse
{
    [JsonPropertyName("id")]
    public long Id { get; init; }
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;
    [JsonPropertyName("is_main")]
    public JsonElement IsMain { get; init; }
    [JsonPropertyName("contact_name")]
    public string? ContactName { get; init; }
    [JsonPropertyName("contact_phone")]
    public string? ContactPhone { get; init; }
    [JsonPropertyName("state")]
    public string? State { get; init; }
    [JsonPropertyName("city")]
    public string? City { get; init; }
    [JsonPropertyName("address")]
    public string? Address { get; init; }
    [JsonPropertyName("phone")]
    public string? Phone { get; init; }
    [JsonPropertyName("tax_number")]
    public string? TaxNumber { get; init; }
    [JsonPropertyName("tax_place")]
    public string? TaxPlace { get; init; }
}
public sealed record ShipmentPackage(
    [property: JsonPropertyName("desi")] string Desi,
    [property: JsonPropertyName("barcode")] string? Barcode,
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("real_desi")] string? RealDesi);
