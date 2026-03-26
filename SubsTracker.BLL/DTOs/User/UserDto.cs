namespace SubsTracker.BLL.DTOs.User;

public class UserDto
{
    public required string IdentityId { get; set; }
    public Guid Id { get; set; }
    public required string FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}
