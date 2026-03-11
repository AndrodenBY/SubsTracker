using System.ComponentModel.DataAnnotations;

namespace SubsTracker.DAL.Entities;

public class UserEntity : BaseEntity
{
    [MaxLength(150)] 
    public string IdentityId { get; set; } = string.Empty;
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? LastName { get; set; }
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    public List<SubscriptionEntity>? Subscriptions { get; set; } = [];
    public List<GroupEntity>? Groups { get; set; } = [];
}
