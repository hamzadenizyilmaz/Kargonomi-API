using System.Text.Json.Serialization;

namespace Kargonomi.Client.Models;

public sealed record ShipmentWriteRequest
{
    [JsonPropertyName("sender_name")]
    public string? SenderName { get; init; }
    [JsonPropertyName("sender_email")]
    public string? SenderEmail { get; init; }
    [JsonPropertyName("sender_tax_number")]
    public string? SenderTaxNumber { get; init; }
    [JsonPropertyName("sender_tax_place")]
    public string? SenderTaxPlace { get; init; }
    [JsonPropertyName("sender_phone")]
    public string? SenderPhone { get; init; }
    [JsonPropertyName("sender_address")]
    public string? SenderAddress { get; init; }
    [JsonPropertyName("sender_state_id")]
    public long? SenderStateId { get; init; }
    [JsonPropertyName("sender_city_id")]
    public long? SenderCityId { get; init; }
    [JsonPropertyName("warehouse_id")]
    public long? WarehouseId { get; init; }
    [JsonPropertyName("buyer_name")]
    public string BuyerName { get; init; } = string.Empty;
    [JsonPropertyName("buyer_email")]
    public string? BuyerEmail { get; init; }
    [JsonPropertyName("buyer_tax_number")]
    public string? BuyerTaxNumber { get; init; }
    [JsonPropertyName("buyer_tax_place")]
    public string? BuyerTaxPlace { get; init; }
    [JsonPropertyName("buyer_phone")]
    public string BuyerPhone { get; init; } = string.Empty;
    [JsonPropertyName("buyer_address")]
    public string BuyerAddress { get; init; } = string.Empty;
    [JsonPropertyName("buyer_state_id")]
    public long BuyerStateId { get; init; }
    [JsonPropertyName("buyer_city_id")]
    public long BuyerCityId { get; init; }
    [JsonPropertyName("packages")]
    public IReadOnlyList<ShipmentPackageInput> Packages { get; init; } = [];

    internal void Validate()
    {
        var errors = new List<string>();
        if (BuyerName.Trim().Length is < 5 or > 255 || BuyerName.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length < 2)
            errors.Add("BuyerName must be 5-255 characters and contain at least two words.");
        if (BuyerPhone.Length != 10 || !BuyerPhone.All(char.IsDigit))
            errors.Add("BuyerPhone must contain exactly 10 digits.");
        if (BuyerAddress.Trim().Length is < 10 or > 512)
            errors.Add("BuyerAddress must be 10-512 characters.");
        if (BuyerStateId <= 0 || BuyerCityId <= 0)
            errors.Add("Buyer state and city IDs must be positive.");
        if (Packages.Count == 0 || Packages.Any(package => package.Desi <= 0))
            errors.Add("At least one package with Desi greater than zero is required.");

        var hasManualSender = new object?[] { SenderName, SenderEmail, SenderTaxNumber, SenderTaxPlace, SenderPhone, SenderAddress, SenderStateId, SenderCityId }.Any(value => value is not null);
        if (WarehouseId is not null && hasManualSender)
            errors.Add("WarehouseId cannot be combined with manual sender fields.");
        if (WarehouseId is null && (string.IsNullOrWhiteSpace(SenderName) || string.IsNullOrWhiteSpace(SenderPhone) || string.IsNullOrWhiteSpace(SenderAddress) || SenderStateId <= 0 || SenderCityId <= 0))
            errors.Add("Manual sender name, phone, address, state and city are required without a warehouse.");
        if (WarehouseId <= 0)
            errors.Add("WarehouseId must be positive when supplied.");

        if (errors.Count > 0)
            throw new ArgumentException(string.Join(" ", errors));
    }
}
public sealed record ShipmentPackageInput(
    [property: JsonPropertyName("desi")] decimal Desi,
    [property: JsonPropertyName("content")] string? Content = null,
    [property: JsonPropertyName("barcode")] string? Barcode = null);
public sealed record ShipmentPatchRequest
{
    [JsonPropertyName("buyer_name"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string?> BuyerName { get; init; }
    [JsonPropertyName("buyer_email"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string?> BuyerEmail { get; init; }
    [JsonPropertyName("buyer_phone"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string?> BuyerPhone { get; init; }
    [JsonPropertyName("buyer_address"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<string?> BuyerAddress { get; init; }
    [JsonPropertyName("buyer_state_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<long?> BuyerStateId { get; init; }
    [JsonPropertyName("buyer_city_id"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<long?> BuyerCityId { get; init; }
    [JsonPropertyName("packages"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Optional<IReadOnlyList<ShipmentPackageInput>?> Packages { get; init; }

    internal void Validate()
    {
        if (!new[] { BuyerName.IsSet, BuyerEmail.IsSet, BuyerPhone.IsSet, BuyerAddress.IsSet, BuyerStateId.IsSet, BuyerCityId.IsSet, Packages.IsSet }.Any(value => value))
            throw new ArgumentException("At least one PATCH field must be supplied.");
        if (BuyerPhone.IsSet && BuyerPhone.Value is { } phone && (phone.Length != 10 || !phone.All(char.IsDigit)))
            throw new ArgumentException("BuyerPhone must contain exactly 10 digits when supplied.");
        if (Packages.IsSet && Packages.Value is { Count: 0 })
            throw new ArgumentException("Packages cannot be an empty list when supplied.");
    }
}
