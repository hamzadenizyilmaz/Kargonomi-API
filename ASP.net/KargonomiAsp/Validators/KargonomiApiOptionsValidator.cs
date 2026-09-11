using Kargonomi.AspNet.Options;
using Microsoft.Extensions.Options;

namespace Kargonomi.AspNet.Validators;

public sealed class KargonomiApiOptionsValidator : IValidateOptions<KargonomiApiOptions>
{
    public ValidateOptionsResult Validate(string? name, KargonomiApiOptions options)
    {
        if (options.BaseUrl is null || !options.BaseUrl.IsAbsoluteUri || options.BaseUrl.Scheme != Uri.UriSchemeHttps)
        {
            return ValidateOptionsResult.Fail("Kargonomi:BaseUrl must be an absolute HTTPS URL.");
        }

        if (options.TimeoutSeconds is < 1 or > 120)
        {
            return ValidateOptionsResult.Fail("Kargonomi:TimeoutSeconds must be between 1 and 120.");
        }

        return ValidateOptionsResult.Success;
    }
}
