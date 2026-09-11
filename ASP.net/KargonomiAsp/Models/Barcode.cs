using System.Text.Json.Serialization;

namespace Kargonomi.Client.Models;

public sealed record BarcodeOptions
{
    public bool? PackageContentVisibility { get; init; }
    public bool? WarningVisibility { get; init; }
    public bool? IntegratedOrderNoVisibility { get; init; }
}
public sealed record BarcodeDocument
{
    internal BarcodeDocument(byte[] bytes, string rawBase64) { Bytes = bytes; RawBase64 = rawBase64; }
    public ReadOnlyMemory<byte> Bytes { get; }
    public string RawBase64 { get; }
}

internal sealed record BarcodeEnvelope([property: JsonPropertyName("data")] string Data);
