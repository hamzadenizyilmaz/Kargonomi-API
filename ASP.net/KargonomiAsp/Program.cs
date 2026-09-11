using Kargonomi.AspNet.Filters;
using Kargonomi.AspNet.Middleware;
using Kargonomi.AspNet.Options;
using Kargonomi.AspNet.Validators;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<KargonomiApiOptions>()
    .Bind(builder.Configuration.GetSection(KargonomiApiOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<KargonomiApiOptions>, KargonomiApiOptionsValidator>();
builder.Services.AddControllers(options => options.Filters.Add<ApiExceptionFilter>());
builder.Services.AddProblemDetails();

var configured = builder.Configuration.GetSection(KargonomiApiOptions.SectionName).Get<KargonomiApiOptions>();
if (!string.IsNullOrWhiteSpace(configured?.ApiToken))
{
    builder.Services.AddKargonomi(options =>
    {
        options.ApiToken = configured.ApiToken;
        options.BaseUri = configured.BaseUrl;
        options.Timeout = TimeSpan.FromSeconds(configured.TimeoutSeconds);
    });
}

var app = builder.Build();
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.MapControllers();
app.Run();

public partial class Program;
