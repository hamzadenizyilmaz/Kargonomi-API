using Kargonomi.Client.Infrastructure;
using Kargonomi.Client.Models;

namespace Kargonomi.Client;

public sealed class ShipmentsResource
{
    private readonly KargonomiHttpPipeline _pipeline;
    internal ShipmentsResource(KargonomiHttpPipeline pipeline) => _pipeline = pipeline;
    public Task<ShipmentPage> ListAsync(int page = 1, CancellationToken cancellationToken = default)
    {
        if (page <= 0) throw new ArgumentOutOfRangeException(nameof(page));
        return _pipeline.GetAsync<ShipmentPage>($"shipments?page={page}", cancellationToken);
    }
    public Task<Shipment> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        return _pipeline.GetAsync<Shipment>($"shipments/{id}", cancellationToken);
    }
    public Task<Shipment> CreateAsync(ShipmentWriteRequest input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.Validate();
        return _pipeline.SendJsonAsync<Shipment>(HttpMethod.Post, "shipments", new { shipment = input }, cancellationToken);
    }
    public Task<Shipment> UpdateAsync(long id, ShipmentWriteRequest input, CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(input);
        input.Validate();
        return _pipeline.SendJsonAsync<Shipment>(HttpMethod.Put, $"shipments/{id}", new { shipment = input }, cancellationToken);
    }
    public Task<Shipment> PatchAsync(long id, ShipmentPatchRequest patch, CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(patch);
        patch.Validate();
        return _pipeline.SendJsonAsync<Shipment>(HttpMethod.Patch, $"shipments/{id}", new { shipment = patch }, cancellationToken);
    }
    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        return _pipeline.SendNoContentAsync(HttpMethod.Delete, $"shipments/{id}", cancellationToken);
    }
    public Task<CancellationRequestResult> CancelAsync(long id, CancellationToken cancellationToken = default)
    {
        ValidateId(id);
        return _pipeline.SendJsonAsync<CancellationRequestResult>(HttpMethod.Post, "shipments/cancel", new { shipment_id = id }, cancellationToken);
    }
    public async IAsyncEnumerable<Shipment> GetAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var pageNumber = 1;
        var visited = new HashSet<int>();
        while (visited.Add(pageNumber))
        {
            var page = await ListAsync(pageNumber, cancellationToken).ConfigureAwait(false);
            foreach (var shipment in page.Items) yield return shipment;
            if (page.Meta.CurrentPage >= page.Meta.LastPage || string.IsNullOrWhiteSpace(page.Links.Next)) yield break;
            pageNumber = page.Meta.CurrentPage + 1;
        }
    }

    internal static void ValidateId(long id)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id), "Kargonomi IDs must be positive.");
    }
}
public sealed record CancellationRequestResult(
    [property: System.Text.Json.Serialization.JsonPropertyName("message")] string Message,
    [property: System.Text.Json.Serialization.JsonPropertyName("shipment_id")] long ShipmentId);
