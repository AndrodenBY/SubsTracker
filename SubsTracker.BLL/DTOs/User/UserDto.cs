using SubsTracker.BLL.DTOs.Subscription;

namespace SubsTracker.BLL.DTOs.User;

public class UserDto : BaseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }
}