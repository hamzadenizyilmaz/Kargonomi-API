using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;

namespace Kargonomi.Client.Models;

public sealed record Webhook
{
    [JsonPropertyName("id")] public long Id { get; init; }
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
    [JsonPropertyName("url")] public Uri Url { get; init; } = new("https://example.invalid");
    [JsonPropertyName("event_type")] public string EventType { get; init; } = string.Empty;
    [JsonPropertyName("is_active")] public bool IsActive { get; init; }
    [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData { get; init; } = new Dictionary<string, JsonElement>();
}
public sealed record WebhookWriteRequest(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] Uri Url,
    [property: JsonPropertyName("event_type")] string EventType,
    [property: JsonPropertyName("is_active")] bool IsActive)
{
    internal void Validate()
    {
        if (Name.Trim().Length is < 2 or > 255) throw new ArgumentException("Webhook name must be 2-255 characters.");
        if (!Url.IsAbsoluteUri || Url.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(Url.UserInfo) || !IsPublicHost(Url.Host)) throw new ArgumentException("Webhook URL must target a public HTTPS host without user information.");
        if (string.IsNullOrWhiteSpace(EventType) || EventType.Length > 255) throw new ArgumentException("Webhook event type is required.");
    }

    private static bool IsPublicHost(string host)
    {
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) || host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase)) return false;
        if (!IPAddress.TryParse(host, out var address)) return host.Contains('.', StringComparison.Ordinal);
        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any) || address.IsIPv6LinkLocal || address.IsIPv6SiteLocal) return false;
        if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) return true;
        var bytes = address.GetAddressBytes();
        return bytes[0] is not 0 and not 10 and not 127 && !(bytes[0] == 169 && bytes[1] == 254) && !(bytes[0] == 172 && bytes[1] is >= 16 and <= 31) && !(bytes[0] == 192 && bytes[1] == 168) && bytes[0] < 224;
    }
}

internal sealed record WebhookListEnvelope([property: JsonPropertyName("data")] IReadOnlyList<Webhook> Data);
