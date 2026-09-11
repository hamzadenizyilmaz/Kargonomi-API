using Kargonomi.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Kargonomi.AspNet.Filters;

public sealed class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is not KargonomiException exception)
        {
            return;
        }

        var status = exception.StatusCode is null
            ? StatusCodes.Status503ServiceUnavailable
            : (int)exception.StatusCode.Value;
        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = "Kargonomi request failed",
            Detail = exception.Message,
            Instance = context.HttpContext.Request.Path,
        })
        {
            StatusCode = status,
        };
        context.ExceptionHandled = true;
    }
}
