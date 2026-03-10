using System.Text.Json.Serialization;

namespace SubsTracker.API.Middlewares.ExceptionHandling;

public class ErrorModel(int statusCode, string? message, object? errors = null)
{
    public int StatusCode { get; set; } = statusCode;
    public string? Message { get; set; } = message;
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Errors { get; set; } = errors;
}
