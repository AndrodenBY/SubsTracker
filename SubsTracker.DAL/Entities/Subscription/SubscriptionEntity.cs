using SubsTracker.DAL.Entities.User;
using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Entities.Subscription;

public class SubscriptionEntity : BaseModel
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
