namespace SubsTracker.API.ViewModel.User.Create;

public class CreateUserViewModel
{
    public string Auth0Id { get; set; } = string.Empty;
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Email { get; set; }
}
