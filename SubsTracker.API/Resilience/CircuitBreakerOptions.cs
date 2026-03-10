using System.ComponentModel.DataAnnotations;

namespace SubsTracker.API.Resilience;

public class CircuitBreakerOptions
{
    public const string SectionName = "CircuitBreaker";
    
    [Required]
    public required double FailureRatio { get; set; }
    [Required]
    public required int SamplingDuration { get; set; }
    [Required]
    public required int MinimumThroughput { get; set; }
    [Required]
    public required int BreakDuration { get; set; }
}
