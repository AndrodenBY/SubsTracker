using System.ComponentModel.DataAnnotations;

namespace SubsTracker.API.Resilience;

public class RetryOptions
{
    public const string SectionName = "Retry";

    [Required]
    public required int MaxRetryAttempts { get; set; }
    [Required]
    public required double SecondsTimeout { get; set; }
    [Required]
    public required double BaseDelaySeconds { get; set; }
}
