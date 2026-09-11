using System.Text.Json.Serialization;

namespace Kargonomi.Client.Models;

public sealed record WarehouseCreateRequest
{
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("is_main")] public bool IsMain { get; init; }
    [JsonPropertyName("contact_name")] public string ContactName { get; init; } = string.Empty;
    [JsonPropertyName("contact_phone")] public string ContactPhone { get; init; } = string.Empty;
    [JsonPropertyName("address")] public string Address { get; init; } = string.Empty;
    [JsonPropertyName("state_id")] public long StateId { get; init; }
    [JsonPropertyName("city_id")] public long CityId { get; init; }
    [JsonPropertyName("tax_number")] public string TaxNumber { get; init; } = string.Empty;

    internal void Validate()
    {
        if (Name.Trim().Length is < 2 or > 255) throw new ArgumentException("Warehouse name must be 2-255 characters.");
        if (string.IsNullOrWhiteSpace(ContactName)) throw new ArgumentException("Contact name is required.");
        if (ContactPhone.Length != 10 || !ContactPhone.All(char.IsDigit)) throw new ArgumentException("Contact phone must contain 10 digits.");
        if (Address.Trim().Length is < 10 or > 512) throw new ArgumentException("Warehouse address must be 10-512 characters.");
        if (StateId <= 0 || CityId <= 0) throw new ArgumentException("Warehouse state and city IDs must be positive.");
        if (TaxNumber.Length is < 10 or > 11 || !TaxNumber.All(char.IsDigit)) throw new ArgumentException("Tax number must contain 10-11 digits.");
    }
}
