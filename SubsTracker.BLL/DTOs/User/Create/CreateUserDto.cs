namespace SubsTracker.BLL.DTOs.User.Create;

public class CreateUserDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
}