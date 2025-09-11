using SubsTracker.Domain.Enums;

namespace SubsTracker.BLL.DTOs.Subscription;

public class CreateSubscriptionDto
{
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public DateOnly DueDate { get; set; }
    public SubscriptionType Type { get; set; }
    public SubscriptionContent Content { get; set; }
}
