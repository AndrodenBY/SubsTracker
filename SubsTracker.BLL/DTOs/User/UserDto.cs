using SubsTracker.BLL.DTOs.Subscription;

namespace SubsTracker.BLL.DTOs;

public class UserDto : BaseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int SubscriptionCount { get; set; }
    public List<SubscriptionDto>? Subscriptions { get; set; }
    public List<UserGroupDto>? Groups { get; set; }
}