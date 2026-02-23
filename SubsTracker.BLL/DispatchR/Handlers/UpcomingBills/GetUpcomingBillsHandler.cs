using AutoMapper;
using DispatchR.Abstractions.Send;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Exceptions;

namespace SubsTracker.BLL.DispatchR.Handlers.UpcomingBills;

public class GetUpcomingBillsHandler(
    IUserRepository userRepository,
    ISubscriptionRepository subscriptionRepository,
    ICacheAccessService cacheAccessService,
    IMapper mapper) 
    : IRequestHandler<GetUpcomingBills, ValueTask<List<SubscriptionDto>>>
{
    public async ValueTask<List<SubscriptionDto>> Handle(GetUpcomingBills request, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByAuth0Id(request.Auth0Id, cancellationToken)
                           ?? throw new UnknownIdentifierException($"User with {request.Auth0Id} not found");
        
        var cacheKey = RedisKeySetter.SetCacheKey(existingUser.Id, "upcoming_bills");
        var cachedData = await cacheAccessService.GetData<List<SubscriptionDto>>(cacheKey, cancellationToken);
        
        if (cachedData is not null) 
            return cachedData;
        
        var billsToPay = await subscriptionRepository.GetUpcomingBills(existingUser.Id, cancellationToken)
                         ?? throw new UnknownIdentifierException($"Subscriptions with UserId {existingUser.Id} not found");
        
        var mappedList = mapper.Map<List<SubscriptionDto>>(billsToPay);
        
        await cacheAccessService.SetData(cacheKey, mappedList, RedisConstants.ExpirationTime, cancellationToken);
        
        return mappedList;
    }
}
