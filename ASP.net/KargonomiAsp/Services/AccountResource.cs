using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kargonomi.Client.Infrastructure;
using Kargonomi.Client.Models;

namespace Kargonomi.Client;

public sealed class AccountResource
{
    private readonly KargonomiHttpPipeline _pipeline;

    internal AccountResource(KargonomiHttpPipeline pipeline) => _pipeline = pipeline;
    public async Task<AccountCredit> GetCreditAsync(CancellationToken cancellationToken = default)
    {
        var response = await _pipeline.GetAsync<CreditEnvelope>("user/credit", cancellationToken).ConfigureAwait(false);
        return new AccountCredit(response.Data.Credit);
    }

    private sealed record CreditEnvelope([property: JsonPropertyName("data")] CreditData Data);

    private sealed record CreditData(
        [property: JsonPropertyName("credit"), JsonConverter(typeof(FlexibleDecimalConverter))] decimal Credit);

    private sealed class FlexibleDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.Number => reader.GetDecimal(),
                JsonTokenType.String when decimal.TryParse(reader.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var value) => value,
                _ => throw new JsonException("Credit must be a decimal number or invariant decimal string.")
            };

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}
