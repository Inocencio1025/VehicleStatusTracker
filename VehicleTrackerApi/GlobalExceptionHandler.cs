using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace VehicleTrackerApi;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path.Value);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An error occurred while processing the request."
        };

        if (_env.IsDevelopment())
        {
            problem.Detail = exception.Message;
        }

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
