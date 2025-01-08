using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace WebAPI.Infrastructure.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            httpContext.Response.StatusCode = exception switch
            {
                Exception => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError,
            };

            _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);
            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Type = exception.GetType().Name,
                    Title = "An error occured",
                    Detail = exception.Message,
                }
            });
        }
    }
}
