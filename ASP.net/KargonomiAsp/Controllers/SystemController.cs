using Kargonomi.AspNet.Base;
using Kargonomi.AspNet.DTOs;
using Kargonomi.AspNet.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Kargonomi.AspNet.Controllers;

[Route("api/system")]
public sealed class SystemController(
    IHostEnvironment environment,
    IOptions<KargonomiApiOptions> options) : ApiControllerBase
{
    [HttpGet("status")]
    [ProducesResponseType<ApiStatusDto>(StatusCodes.Status200OK)]
    public ActionResult<ApiStatusDto> GetStatus() => Ok(new ApiStatusDto(
        "Kargonomi ASP.NET API",
        environment.EnvironmentName,
        !string.IsNullOrWhiteSpace(options.Value.ApiToken),
        DateTimeOffset.UtcNow));
}
