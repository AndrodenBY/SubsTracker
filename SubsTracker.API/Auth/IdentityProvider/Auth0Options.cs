using System.ComponentModel.DataAnnotations;

namespace SubsTracker.API.Auth.IdentityProvider;

public class Auth0Options
{
    public const string SectionName = "Auth0";

    [Required]
    public required string Domain { get; set; }
    [Required]
    public required string ClientId { get; set; }
    [Required]
    public required string ClientSecret { get; set; }
    [Required]
    public required string Audience { get; set; }
    [Required]
    public required string Authority { get; set; }
    [Required]
    public required string ManagementApiUrl { get; set; }
}
