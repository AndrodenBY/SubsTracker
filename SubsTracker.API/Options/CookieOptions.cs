using System.ComponentModel.DataAnnotations;

namespace SubsTracker.API.Options;

public class CookieOptions
{
    public const string SectionName = "Cookie";
    
    [Required]
    public required string  Name { get; set; }
    [Required]
    public required bool  HttpOnly { get; set; }
    [Required]
    public required SameSiteMode  SameSite { get; set; }
    [Required]
    public required CookieSecurePolicy  SecurePolicy { get; set; }
    [Required]
    public required string Path  { get; set; }
    [Required]
    public required bool IsEssential  { get; set; }
    [Required]
    public required string Domain { get; set; }
    [Required]
    public required int ExpirationTimeSpan { get; set; }
    [Required]
    public required bool SlidingExpiration { get; set; }
    
}
