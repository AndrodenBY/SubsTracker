using System.ComponentModel.DataAnnotations;

namespace SubsTracker.API.Options;

public class CorsOptions
{
    public const string SectionName = "Cors";
    
    [Required]
    public required string[] AllowedOrigins { get; set; }
}
