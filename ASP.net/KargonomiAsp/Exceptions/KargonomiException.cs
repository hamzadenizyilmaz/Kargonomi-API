using System.Net;

namespace Kargonomi.Client;

public class KargonomiException : Exception
{
    internal KargonomiException(string message, HttpStatusCode? statusCode = null, bool retryable = false, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Retryable = retryable;
    }
    public HttpStatusCode? StatusCode { get; }
    public bool Retryable { get; }
}
public sealed class KargonomiAuthenticationException : KargonomiException
{
    internal KargonomiAuthenticationException(HttpStatusCode statusCode) : base("Kargonomi authentication failed.", statusCode) { }
}
public sealed class KargonomiValidationException : KargonomiException
{
    internal KargonomiValidationException(HttpStatusCode statusCode, IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors)
        : base("Kargonomi rejected one or more request fields.", statusCode) => FieldErrors = fieldErrors;
    public IReadOnlyDictionary<string, IReadOnlyList<string>> FieldErrors { get; }
}
public sealed class KargonomiNotFoundException : KargonomiException
{
    internal KargonomiNotFoundException() : base("The Kargonomi resource was not found.", HttpStatusCode.NotFound) { }
}
public sealed class KargonomiRateLimitException : KargonomiException
{
    internal KargonomiRateLimitException(TimeSpan? retryAfter) : base("Kargonomi rate limit reached.", HttpStatusCode.TooManyRequests, true) => RetryAfter = retryAfter;
    public TimeSpan? RetryAfter { get; }
}
public sealed class KargonomiTimeoutException : KargonomiException
{
    internal KargonomiTimeoutException(Exception innerException) : base("Kargonomi request timed out.", null, true, innerException) { }
}
public sealed class KargonomiNetworkException : KargonomiException
{
    internal KargonomiNetworkException(Exception innerException) : base("Kargonomi could not be reached.", null, true, innerException) { }
}
