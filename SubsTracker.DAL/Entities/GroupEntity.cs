using System.ComponentModel.DataAnnotations;

namespace SubsTracker.DAL.Entities;

public class GroupEntity : BaseEntity
{
    [MaxLength(100)] 
    public string Name { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public UserEntity? User { get; set; }
    public List<MemberEntity>? Members { get; set; } = [];
    public List<SubscriptionEntity>? SharedSubscriptions { get; set; } = [];
}
