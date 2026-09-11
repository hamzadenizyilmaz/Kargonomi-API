namespace Kargonomi.Client;

public sealed class KargonomiServiceOptions
{
    public string ApiToken { get; set; } = string.Empty;

    public Uri BaseUri { get; set; } = null!;

    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    internal KargonomiClientOptions ToClientOptions() => new(ApiToken, BaseUri) { Timeout = Timeout };
}
