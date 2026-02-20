using System.Diagnostics.CodeAnalysis;

namespace SubsTracker.API.ViewModel;

[ExcludeFromCodeCoverage]
public class GroupViewModel
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}
