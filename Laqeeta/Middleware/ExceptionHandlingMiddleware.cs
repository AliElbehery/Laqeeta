using Laqeeta.Core.Exceptions;

namespace Laqeeta.Api.Middleware;

/// <summary>Maps Core exceptions to HTTP status codes so controllers stay clean.</summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (context.Request.Path.StartsWithSegments("/api"))
        {
            var (status, title) = ex switch
            {
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                NotFoundException     => (StatusCodes.Status404NotFound, "Not found"),
                ConflictException     => (StatusCodes.Status409Conflict, "Conflict"),
                ForbiddenException    => (StatusCodes.Status403Forbidden, "Forbidden"),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violated"),
                _                     => (StatusCodes.Status500InternalServerError, "Unexpected error")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Unhandled exception");

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new
            {
                title,
                status,
                detail = status != StatusCodes.Status500InternalServerError ? ex.Message
                       : env.IsDevelopment() ? ex.ToString()      // dev only: show the real error
                       : "Something went wrong."
            });
        }
    }
}
