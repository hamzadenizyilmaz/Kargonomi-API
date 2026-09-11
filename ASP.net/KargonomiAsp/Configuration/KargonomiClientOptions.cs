namespace Kargonomi.Client;

public sealed record KargonomiClientOptions
{
    public KargonomiClientOptions(string apiToken, Uri baseUri)
    {
        if (string.IsNullOrWhiteSpace(apiToken))
        {
            throw new ArgumentException("An API token is required.", nameof(apiToken));
        }

        ApiToken = apiToken.Trim();
        BaseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
    }

    public string ApiToken { get; }

    public Uri BaseUri { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}
