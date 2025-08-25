using System.ComponentModel.DataAnnotations;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.BLL.DTOs;

public class UpdateUserDto: BaseDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public int? SubscriptionCount { get; set; }
    public List<Subscription.SubscriptionDto>? Subscriptions { get; set; }
    public List<UserGroupDto>? Groups { get; set; }
}