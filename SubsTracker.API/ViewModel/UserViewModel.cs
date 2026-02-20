namespace SubsTracker.API.ViewModel;

public class UserViewModel
{
    public required string Auth0Id { get; set; }
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}
