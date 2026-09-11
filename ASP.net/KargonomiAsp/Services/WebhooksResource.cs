using Kargonomi.Client.Infrastructure;
using Kargonomi.Client.Models;

namespace Kargonomi.Client;

public sealed class WebhooksResource
{
    private readonly KargonomiHttpPipeline _pipeline;
    internal WebhooksResource(KargonomiHttpPipeline pipeline) => _pipeline = pipeline;
    public async Task<IReadOnlyList<Webhook>> ListAsync(CancellationToken cancellationToken = default) =>
        (await _pipeline.GetAsync<WebhookListEnvelope>("webhooks", cancellationToken).ConfigureAwait(false)).Data;
    public Task<Webhook> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        ShipmentsResource.ValidateId(id);
        return _pipeline.GetAsync<Webhook>($"webhooks/{id}", cancellationToken);
    }
    public Task<Webhook> CreateAsync(WebhookWriteRequest input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input); input.Validate();
        return _pipeline.SendJsonAsync<Webhook>(HttpMethod.Post, "webhooks", input, cancellationToken);
    }
    public Task<Webhook> UpdateAsync(long id, WebhookWriteRequest input, CancellationToken cancellationToken = default)
    {
        ShipmentsResource.ValidateId(id); ArgumentNullException.ThrowIfNull(input); input.Validate();
        return _pipeline.SendJsonAsync<Webhook>(HttpMethod.Put, $"webhooks/{id}", input, cancellationToken);
    }
    public Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        ShipmentsResource.ValidateId(id);
        return _pipeline.SendNoContentAsync(HttpMethod.Delete, $"webhooks/{id}", cancellationToken);
    }
}
