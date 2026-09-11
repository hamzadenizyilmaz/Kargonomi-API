using Microsoft.Extensions.Primitives;

namespace Kargonomi.AspNet.Middleware;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-ID";
    public async Task InvokeAsync(HttpContext context)
    {
        var supplied = context.Request.Headers[HeaderName].ToString();
        var correlationId = supplied.Length is > 0 and <= 128
            ? supplied
            : Guid.NewGuid().ToString("N");

        context.TraceIdentifier = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = new StringValues(correlationId);
            return Task.CompletedTask;
        });
        await next(context).ConfigureAwait(false);
    }
}
