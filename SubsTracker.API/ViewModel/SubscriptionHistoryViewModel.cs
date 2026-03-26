using SubsTracker.Domain.Enums;

namespace SubsTracker.API.ViewModel;

public class SubscriptionHistoryViewModel
{
    public Guid Id { get; set; }
    public SubscriptionAction Action { get; set; }
    public decimal? PricePaid { get; set; }
    public DateTime CreatedAt { get; set; }
}
