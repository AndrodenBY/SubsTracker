using System.ComponentModel.DataAnnotations;

namespace SubsTracker.API.Resilience;

public class CapacityLimiterOptions
{
    public const string SectionName = "CapacityLimiter";
    
    [Required]
    public required int PermitLimit { get; set; }
    [Required]
    public required int RequestWindow { get; set; }
    [Required]
    public required int SegmentsPerWindow { get; set; }
}

