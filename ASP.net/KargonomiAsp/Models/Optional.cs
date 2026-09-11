using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kargonomi.Client.Models;

[JsonConverter(typeof(OptionalJsonConverterFactory))]
public readonly record struct Optional<T>
{
    private Optional(T value)
    {
        Value = value;
        IsSet = true;
    }
    public bool IsSet { get; }
    public T? Value { get; }
    public static Optional<T> From(T value) => new(value);
}

internal sealed class OptionalJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        return (JsonConverter)Activator.CreateInstance(typeof(OptionalJsonConverter<>).MakeGenericType(valueType))!;
    }

    private sealed class OptionalJsonConverter<T> : JsonConverter<Optional<T>>
    {
        public override Optional<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            Optional<T>.From(JsonSerializer.Deserialize<T>(ref reader, options)!);

        public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
        {
            if (!value.IsSet)
            {
                throw new JsonException("An unset optional value must be omitted by the containing model.");
            }

            JsonSerializer.Serialize(writer, value.Value, options);
        }
    }
}
