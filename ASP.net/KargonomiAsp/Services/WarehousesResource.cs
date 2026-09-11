using Kargonomi.Client.Infrastructure;
using Kargonomi.Client.Models;

namespace Kargonomi.Client;

public sealed class WarehousesResource
{
    private readonly KargonomiHttpPipeline _pipeline;
    internal WarehousesResource(KargonomiHttpPipeline pipeline) => _pipeline = pipeline;
    public Task<Warehouse> CreateAsync(WarehouseCreateRequest input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        input.Validate();
        return _pipeline.SendJsonAsync<Warehouse>(HttpMethod.Post, "warehouses", new { warehouse = input }, cancellationToken);
    }
}
