using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kargonomi.Client.Infrastructure;

internal sealed class KargonomiHttpPipeline : IDisposable
{
    private const int MaximumErrorBodyCharacters = 64 * 1024;
    private readonly HttpClient _httpClient;
    private readonly KargonomiClientOptions _options;
    private int _disposed;

    public KargonomiHttpPipeline(HttpClient httpClient, KargonomiClientOptions options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options;

        if (!_options.BaseUri.IsAbsoluteUri || _options.BaseUri.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("BaseUri must be an absolute HTTPS URI.", nameof(options));
        if (_options.Timeout <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(options), "Timeout must be positive.");
    }

    internal static JsonSerializerOptions JsonOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public Task<T> GetAsync<T>(string relativePath, CancellationToken cancellationToken) =>
        SendAsync<T>(HttpMethod.Get, relativePath, null, cancellationToken);

    public Task<T> SendJsonAsync<T>(HttpMethod method, string relativePath, object body, CancellationToken cancellationToken) =>
        SendAsync<T>(method, relativePath, JsonContent.Create(body, options: JsonOptions), cancellationToken);

    public async Task<T> SendMultipartAsync<T>(string relativePath, IReadOnlyDictionary<string, string> fields, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        foreach (var field in fields)
            content.Add(new StringContent(field.Value), field.Key);
        return await SendAsync<T>(HttpMethod.Post, relativePath, content, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendNoContentAsync(HttpMethod method, string relativePath, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, relativePath, null);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Timeout);
        try
        {
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ThrowProviderErrorAsync(response, timeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException error) when (!cancellationToken.IsCancellationRequested)
        {
            throw new KargonomiTimeoutException(error);
        }
        catch (HttpRequestException error)
        {
            throw new KargonomiNetworkException(error);
        }
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string relativePath, HttpContent? content, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, relativePath, content);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(_options.Timeout);
        try
        {
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                await ThrowProviderErrorAsync(response, timeout.Token).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NoContent)
                throw new JsonException("Kargonomi returned 204 for an operation that requires a response body.");

            await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token).ConfigureAwait(false);
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions, timeout.Token).ConfigureAwait(false)
                ?? throw new JsonException("Kargonomi returned an empty JSON response.");
        }
        catch (OperationCanceledException error) when (!cancellationToken.IsCancellationRequested)
        {
            throw new KargonomiTimeoutException(error);
        }
        catch (HttpRequestException error)
        {
            throw new KargonomiNetworkException(error);
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string relativePath, HttpContent? content)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
        var request = new HttpRequestMessage(method, Resolve(relativePath)) { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("kargonomi-dotnet/0.1.0");
        return request;
    }

    private async Task ThrowProviderErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var status = response.StatusCode;
        if (status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            throw new KargonomiAuthenticationException(status);
        if (status == HttpStatusCode.NotFound)
            throw new KargonomiNotFoundException();
        if (status == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta;
            if (retryAfter is null && response.Headers.RetryAfter?.Date is { } date)
                retryAfter = date - DateTimeOffset.UtcNow;
            throw new KargonomiRateLimitException(retryAfter.HasValue && retryAfter.Value > TimeSpan.Zero ? retryAfter : null);
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (raw.Length > MaximumErrorBodyCharacters)
            raw = raw[..MaximumErrorBodyCharacters];
        if (status == HttpStatusCode.UnprocessableEntity)
            throw new KargonomiValidationException(status, ParseFieldErrors(raw));

        var retryable = (int)status >= 500 || status is HttpStatusCode.RequestTimeout or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout;
        throw new KargonomiException($"Kargonomi request failed with HTTP {(int)status}.", status, retryable);
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ParseFieldErrors(string raw)
    {
        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
        try
        {
            using var document = JsonDocument.Parse(raw);
            if (!document.RootElement.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Object)
                return result;
            foreach (var field in errors.EnumerateObject())
            {
                result[field.Name] = field.Value.ValueKind == JsonValueKind.Array
                    ? field.Value.EnumerateArray().Where(value => value.ValueKind == JsonValueKind.String).Select(value => value.GetString()!).ToArray()
                    : ["Invalid value."];
            }
        }
        catch (JsonException)
        {

        }
        return result;
    }

    private Uri Resolve(string relativePath)
    {
        var resolved = new Uri(_options.BaseUri, relativePath.TrimStart('/'));
        if (!string.Equals(resolved.Scheme, _options.BaseUri.Scheme, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(resolved.Host, _options.BaseUri.Host, StringComparison.OrdinalIgnoreCase) ||
            resolved.Port != _options.BaseUri.Port)
            throw new InvalidOperationException("Resolved endpoint escaped the configured Kargonomi origin.");
        return resolved;
    }

    public void Dispose() => Interlocked.Exchange(ref _disposed, 1);
}
