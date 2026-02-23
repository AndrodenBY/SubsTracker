using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Entities;

public class SubscriptionEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public required decimal Price { get; set; }
    public Guid? UserId { get; set; }
    public UserEntity? User { get; set; }
    public required DateOnly DueDate { get; set; }
    public bool Active { get; set; } = true;
    public required SubscriptionType Type { get; set; }
    public required SubscriptionContent Content { get; set; }
    public List<SubscriptionHistory> History { get; set; } = new();
}
