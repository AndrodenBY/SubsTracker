using System.Net;
using System.Net.Mime;
using SubsTracker.Domain.Exceptions;
using InvalidOperationException = SubsTracker.Domain.Exceptions.InvalidOperationException;

namespace SubsTracker.API.Middlewares.ExceptionHandling;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception at {Method} {Path}{Query}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.Request.QueryString);

            await HandleExceptionResponse(httpContext, ex);
        }
    }

    private static async Task HandleExceptionResponse(HttpContext context, Exception exception)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var errorModel = exception switch
        {
            NotFoundException ex => new ErrorModel((int)HttpStatusCode.NotFound, ex.Message),
            ValidationException ex => new ErrorModel((int)HttpStatusCode.BadRequest, ex.Message),
            InvalidOperationException ex => new ErrorModel((int)HttpStatusCode.BadRequest, ex.Message),
            _ => new ErrorModel((int)HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = errorModel.StatusCode;
        await context.Response.WriteAsJsonAsync(errorModel);
    }
}