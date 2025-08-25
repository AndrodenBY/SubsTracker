namespace SubsTracker.API.ExceptionHandling;

public class ErrorModel(int statusCode, string? message, string? details = null)
{
    public int StatusCode { get; set; } = statusCode;
    public string? Message { get; set; } = message;
}