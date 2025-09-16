using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.API.ViewModel.Subscription;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.Domain.Filter;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController(
    ISubscriptionService service, 
    IMapper mapper
    ) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<SubscriptionViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(getById);
    }

    [HttpGet("bills/users/{userId:guid}")]
    public async Task<List<SubscriptionViewModel>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken)
    {
        var getUpcomingBills = await service.GetUpcomingBills(userId, cancellationToken);
        return mapper.Map<List<SubscriptionViewModel>>(getUpcomingBills);
    }
    
    [HttpGet]
    public async Task<List<SubscriptionViewModel>> GetAll([FromQuery] SubscriptionFilterDto? filterDto, CancellationToken cancellationToken)
    {
        var entities = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<SubscriptionViewModel>>(entities);
    }

    
    [HttpPost("{userId:guid}")]
    public async Task<SubscriptionViewModel> Create(Guid userId, [FromBody] CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(userId, createDto, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(create);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<SubscriptionViewModel> Update(Guid id, [FromBody] UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(update);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.Delete(id, cancellationToken);
    }

    [HttpPut("{subscriptionId:guid}/renew")]
    public async Task<SubscriptionViewModel> RenewSubscription(Guid subscriptionId, int monthsToRenew,
        CancellationToken cancellationToken)
    {
        var renew = await service.RenewSubscription(subscriptionId, monthsToRenew, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(renew);
    }
}
