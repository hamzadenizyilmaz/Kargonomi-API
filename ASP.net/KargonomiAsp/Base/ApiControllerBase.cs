using Microsoft.AspNetCore.Mvc;

namespace Kargonomi.AspNet.Base;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase;