using SubsTracker.Domain.Enums;

namespace SubsTracker.API.ViewModel.Subscription;

public class CreateSubscriptionViewModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateOnly DueDate { get; set; }
    public SubscriptionType Type { get; set; }
    public SubscriptionContent Content { get; set; }
}