using Kargonomi.Client.Infrastructure;
using Kargonomi.Client.Models;

namespace Kargonomi.Client;

public sealed class BarcodesResource
{
    private const int MaximumPdfBytes = 25 * 1024 * 1024;
    private readonly KargonomiHttpPipeline _pipeline;
    internal BarcodesResource(KargonomiHttpPipeline pipeline) => _pipeline = pipeline;
    public async Task<BarcodeDocument> GetPdfAsync(long shipmentId, BarcodeOptions? options = null, CancellationToken cancellationToken = default)
    {
        ShipmentsResource.ValidateId(shipmentId);
        options ??= new BarcodeOptions();
        var query = new List<string> { "format=pdf" };
        AddBoolean(query, "packageContentVisibility", options.PackageContentVisibility);
        AddBoolean(query, "warningVisibility", options.WarningVisibility);
        AddBoolean(query, "integratedOrderNoVisibility", options.IntegratedOrderNoVisibility);
        var envelope = await _pipeline.GetAsync<BarcodeEnvelope>($"shipments/{shipmentId}/barcode?{string.Join('&', query)}", cancellationToken).ConfigureAwait(false);
        if (envelope.Data.Length > MaximumPdfBytes * 2) throw new InvalidDataException("Barcode Base64 exceeds the safety limit.");

        byte[] bytes;
        try { bytes = Convert.FromBase64String(envelope.Data); }
        catch (FormatException error) { throw new InvalidDataException("Kargonomi returned invalid barcode Base64.", error); }
        if (bytes.Length > MaximumPdfBytes || bytes.Length < 4 || bytes[0] != '%' || bytes[1] != 'P' || bytes[2] != 'D' || bytes[3] != 'F')
            throw new InvalidDataException("Kargonomi barcode content is not a valid PDF header or exceeds the safety limit.");
        return new BarcodeDocument(bytes, envelope.Data);
    }

    private static void AddBoolean(List<string> query, string name, bool? value)
    {
        if (value.HasValue) query.Add($"{name}={value.Value.ToString().ToLowerInvariant()}");
    }
}
