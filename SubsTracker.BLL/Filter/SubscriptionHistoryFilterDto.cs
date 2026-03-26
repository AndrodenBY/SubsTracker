using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Filter;

public class SubscriptionHistoryFilterDto
{
    public decimal? PricePaid { get; set; }
    public SubscriptionAction? Action { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? SubscriptionName { get; set; }
    public bool? SubscriptionActive { get; set; }
    public SubscriptionType? SubscriptionType { get; set; }
    public SubscriptionContent? SubscriptionContent { get; set; }
}
