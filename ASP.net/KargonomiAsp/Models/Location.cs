using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kargonomi.Client.Models;

public sealed record Location
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("state_id")] public long? StateId { get; init; }
    [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData { get; init; } = new Dictionary<string, JsonElement>();
}

internal sealed record LocationEnvelope([property: JsonPropertyName("data")] IReadOnlyList<Location> Data);
