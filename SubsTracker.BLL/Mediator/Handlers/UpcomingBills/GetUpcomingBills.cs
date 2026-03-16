using DispatchR.Abstractions.Send;
using SubsTracker.BLL.DTOs.Subscription;

namespace SubsTracker.BLL.Mediator.Handlers.UpcomingBills;

public record GetUpcomingBills(Guid UserId) : IRequest<GetUpcomingBills, ValueTask<List<SubscriptionDto>>>;
