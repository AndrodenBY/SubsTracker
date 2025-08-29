using SubsTracker.Domain.Enums;

namespace SubsTracker.API.ViewModel.Subscription;

public class SubscriptionViewModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public DateOnly DueDate { get; set; }
    public SubscriptionType Type { get; set; }
    public SubscriptionContent Content { get; set; }
}
