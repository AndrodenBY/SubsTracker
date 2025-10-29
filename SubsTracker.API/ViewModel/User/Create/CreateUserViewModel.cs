namespace SubsTracker.API.ViewModel.User.Create;

public class CreateUserViewModel
{
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Email { get; set; }
}