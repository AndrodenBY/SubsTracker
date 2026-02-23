using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Entities;

public class SubscriptionHistory : BaseEntity
{
    public required Guid SubscriptionId { get; set; }
    public SubscriptionEntity? Subscription { get; set; }
    public required SubscriptionAction Action { get; set; }
    public decimal? PricePaid { get; set; }
}
