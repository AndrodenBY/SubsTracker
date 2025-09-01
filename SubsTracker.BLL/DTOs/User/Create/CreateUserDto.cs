namespace SubsTracker.BLL.DTOs.User.Create;

public class CreateUserDto
{
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Email { get; set; }
}