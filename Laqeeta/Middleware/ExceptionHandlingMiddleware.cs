using Laqeeta.Core.Exceptions;
using Laqeeta.Core.Localization;

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
                NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violated"),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Unhandled exception");

            // business errors are translated: send "Accept-Language: ar" to get Arabic (default English)
            var lang = Messages.Normalize(context.Request.Headers.AcceptLanguage.ToString());
            var domain = ex as DomainException;

            context.Response.StatusCode = status;
            await context.Response.WriteAsJsonAsync(new
            {
                title,
                status,
                code = domain?.Code,
                detail = domain is not null ? Messages.Error(domain, lang)
                       : env.IsDevelopment() ? ex.ToString()      // dev only: show the real error
                       : "Something went wrong."
            });
        }
    }
}