using SubsTracker.Domain.Enums;

namespace SubsTracker.DAL.Models.Subscription;

public class SubscriptionHistory : BaseModel
{
    public Guid SubscriptionId { get; set; }
    public Subscription? Subscription { get; set; }
    public SubscriptionAction Action { get; set; }
    public decimal? PricePaid { get; set; }
}
