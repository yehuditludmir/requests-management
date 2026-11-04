using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace RequestsManagement.API.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        // תופס כל חריגה שלא טופלה: רושם אותה ללוג ומחזיר 500 עם traceId, בלי פרטים פנימיים
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation("Request was cancelled by the client");
                httpContext.Response.StatusCode = 499;
                return true;
            }

            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred",
                    Extensions = { ["traceId"] = traceId }
                }
            });
        }
    }
}
