using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kargonomi.Client.Models;

[JsonConverter(typeof(ShipmentStatusJsonConverter))]
public readonly record struct ShipmentStatus(string Value)
{
    private static readonly HashSet<string> KnownValues = new(StringComparer.Ordinal)
    {
        "draft", "ready", "webservice_order_failed", "webservice_order_creating",
        "webservice_order_created", "webservice_checking_shipment", "webservice_shipment_started",
        "webservice_shipment_delivered", "webservice_shipment_not_delivered",
        "webservice_shipment_returning", "webservice_shipment_missing", "cancelled",
        "request_for_cancellation"
    };
    public bool IsKnown => KnownValues.Contains(Value);
    public override string ToString() => Value;
}

internal sealed class ShipmentStatusJsonConverter : JsonConverter<ShipmentStatus>
{
    public override ShipmentStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new(reader.GetString() ?? throw new JsonException("Shipment status cannot be null."));

    public override void Write(Utf8JsonWriter writer, ShipmentStatus value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.Value);
}
