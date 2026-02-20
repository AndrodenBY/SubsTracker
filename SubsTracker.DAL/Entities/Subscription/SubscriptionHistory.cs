using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Entities.Subscription;

public class SubscriptionHistory : BaseModel
{
    public required Guid SubscriptionId { get; set; }
    public SubscriptionEntity? Subscription { get; set; }
    public required SubscriptionAction Action { get; set; }
    public decimal? PricePaid { get; set; }
}
