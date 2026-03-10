using System.Net;
using System.Net.Mime;
using FluentValidation;
using SubsTracker.Domain.Exceptions;

namespace SubsTracker.API.Middlewares.ExceptionHandling;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    
    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception exception)
        {
            if (exception is ValidationException fluentException)
            {
                var validationErrors = fluentException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                
                logger.LogWarning("Validation failed at {Path} | Errors: {@ValidationErrors}", 
                    httpContext.Request.Path, 
                    validationErrors);

                await HandleValidationResponse(httpContext, validationErrors);
            }
            else
            {
                logger.LogError(exception, "Unhandled Exception at {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);

                await HandleExceptionResponse(httpContext, exception);
            }
        }
    }
    
    private static async Task HandleValidationResponse(HttpContext context, IDictionary<string, string[]> errors)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        var errorModel = new ErrorModel(context.Response.StatusCode, "One or more validation errors occurred.", errors);
        await context.Response.WriteAsJsonAsync(errorModel);
    }

    private static async Task HandleExceptionResponse(HttpContext context, Exception exception)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var errorModel = exception switch
        {
            UnknownIdentifierException ex => new ErrorModel((int)HttpStatusCode.NotFound, ex.Message),
            InvalidRequestDataException ex => new ErrorModel((int)HttpStatusCode.BadRequest, ex.Message),
            PolicyViolationException ex => new ErrorModel((int)HttpStatusCode.BadRequest, ex.Message),
            ForbiddenException ex => new ErrorModel((int)HttpStatusCode.Forbidden, ex.Message),
            _ => new ErrorModel((int)HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.StatusCode = errorModel.StatusCode;
        await context.Response.WriteAsJsonAsync(errorModel);
    }
}
