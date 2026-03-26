using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.Subscription;

public class SubscriptionHistoryDto
{
    public Guid Id { get; set; }
    public SubscriptionAction Action { get; set; }
    public decimal? PricePaid { get; set; }
    public DateTime CreatedAt { get; set; }
}
