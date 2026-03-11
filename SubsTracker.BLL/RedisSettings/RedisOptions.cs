using System.ComponentModel.DataAnnotations;

namespace SubsTracker.BLL.RedisSettings;

public class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public required string ConnectionString { get; set; }
    [Required]
    public required string InstanceName { get; set; } = "Redis_";
}
