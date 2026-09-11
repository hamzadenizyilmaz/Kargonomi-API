using Kargonomi.Client.Infrastructure;
using Kargonomi.Client.Models;

namespace Kargonomi.Client;

public sealed class LocationsResource
{
    private readonly KargonomiHttpPipeline _pipeline;
    internal LocationsResource(KargonomiHttpPipeline pipeline) => _pipeline = pipeline;
    public async Task<IReadOnlyList<Location>> GetStatesAsync(long? countryId = null, CancellationToken cancellationToken = default)
    {
        if (countryId <= 0) throw new ArgumentOutOfRangeException(nameof(countryId));
        var path = countryId is null ? "states" : $"states/{countryId}";
        return (await _pipeline.GetAsync<LocationEnvelope>(path, cancellationToken).ConfigureAwait(false)).Data;
    }
    public async Task<IReadOnlyList<Location>> GetCitiesAsync(long stateId, CancellationToken cancellationToken = default)
    {
        ShipmentsResource.ValidateId(stateId);
        return (await _pipeline.GetAsync<LocationEnvelope>($"cities/{stateId}", cancellationToken).ConfigureAwait(false)).Data;
    }
}
