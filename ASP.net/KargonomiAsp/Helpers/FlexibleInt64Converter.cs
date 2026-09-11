using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kargonomi.Client.Helpers;

internal sealed class FlexibleInt64Converter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => reader.TokenType switch
    {
        JsonTokenType.Number => reader.GetInt64(),
        JsonTokenType.String when long.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) => value,
        _ => throw new JsonException("Identifier must be an integer or integer string.")
    };

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
}
