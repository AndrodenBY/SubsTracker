using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.Filter;

public class SubscriptionFilter
{
    public string? Name { get; set; }
    public Guid? Id { get; set; }
    public Guid? UserId { get; set; }
    public decimal? Price { get; set; }
    public SubscriptionType? Type { get; set; }
    public SubscriptionContent? Content { get; set; }
    public bool? Active { get; set; }
}
