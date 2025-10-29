namespace SubsTracker.API.Middlewares.ExceptionHandling;

public class ErrorModel(int statusCode, string? message)
{
    public int StatusCode { get; set; } = statusCode;
    public string? Message { get; set; } = message;
}