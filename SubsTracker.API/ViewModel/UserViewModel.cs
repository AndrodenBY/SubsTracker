using System.Diagnostics.CodeAnalysis;

namespace SubsTracker.API.ViewModel;

[ExcludeFromCodeCoverage]
public class UserViewModel
{
    public required string Auth0Id { get; set; }
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}
