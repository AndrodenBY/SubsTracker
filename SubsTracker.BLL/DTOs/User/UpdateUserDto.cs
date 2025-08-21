using System.ComponentModel.DataAnnotations;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.BLL.DTOs;

public class UpdateUserDto: BaseDto
{
    [MaxLength(50)]
    public string? FirstName { get; set; }
    [MaxLength(50)]
    public string? LastName { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    [Range(0, int.MaxValue)]
    public int? SubscriptionCount { get; set; }
    public List<Subscription.SubscriptionDto>? Subscriptions { get; set; }
    public List<UserGroupDto>? Groups { get; set; }
}