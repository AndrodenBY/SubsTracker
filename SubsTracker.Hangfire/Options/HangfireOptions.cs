using System.ComponentModel.DataAnnotations;

namespace SubsTracker.Hangfire.Options;

public class HangfireOptions
{
    public const string SectionName = "Hangfire";
    
    [Required]
    public required string DatabaseName { get; set; }
    [Required]
    public required string CollectionName { get; set; }
}
