namespace Kargonomi.AspNet.Options;

public sealed class KargonomiApiOptions
{
    public const string SectionName = "Kargonomi";

    public string ApiToken { get; set; } = string.Empty;

    public Uri BaseUrl { get; set; } = null!;

    public int TimeoutSeconds { get; set; } = 30;
}
