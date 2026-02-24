using System.Diagnostics.CodeAnalysis;

namespace SubsTracker.API.ViewModel;

[ExcludeFromCodeCoverage]
public class GroupViewModel
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public List<SubscriptionViewModel>? SharedSubscriptions { get; set; }
}
