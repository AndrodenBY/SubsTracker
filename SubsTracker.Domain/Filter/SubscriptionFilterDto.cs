using SubsTracker.Domain.Enums;

namespace SubsTracker.Domain.Filter;

public class SubscriptionFilterDto
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public SubscriptionType? Type { get; set; }
    public SubscriptionContent? Content { get; set; }
}
