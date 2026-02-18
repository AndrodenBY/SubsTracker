using DispatchR.Abstractions.Send;
using SubsTracker.BLL.DTOs.Subscription;

namespace SubsTracker.BLL.Handlers.UpcomingBills;

public record GetUpcomingBills(string Auth0Id) : IRequest<GetUpcomingBills, ValueTask<List<SubscriptionDto>>>;
