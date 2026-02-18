using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.Extension;
using SubsTracker.API.ViewModel.Subscription;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController(
    ISubscriptionService subscriptionService,
    IMapper mapper
) : ControllerBase
{
    /// <summary>
    ///     Retrieves a subscription by its ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<SubscriptionViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await subscriptionService.GetUserInfoById(id, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(getById);
    }

    /// <summary>
    ///     Retrieves all subscriptions with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<PaginatedList<SubscriptionViewModel>> GetAll([FromQuery] SubscriptionFilterDto? filterDto, [FromQuery] PaginationParameters? paginationParameters, CancellationToken cancellationToken)
    {
        var pagedResult = await subscriptionService.GetAll(filterDto, paginationParameters, cancellationToken);
        return pagedResult.MapToPage(mapper.Map<SubscriptionViewModel>);
    }

    /// <summary>
    ///     Creates a new subscription for a specific user
    /// </summary>
    [HttpPost]
    public async Task<SubscriptionViewModel> Create([FromBody] CreateSubscriptionDto createDto, CancellationToken cancellationToken)
    {
        var create = await subscriptionService.Create(User.GetAuth0IdFromToken(), createDto, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(create);
    }

    /// <summary>
    ///     Updates an existing subscription
    /// </summary>
    [HttpPut]
    public async Task<SubscriptionViewModel> Update([FromBody] UpdateSubscriptionDto updateDto, CancellationToken cancellationToken)
    {
        var update = await subscriptionService.Update(User.GetAuth0IdFromToken(), updateDto, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(update);
    }

    /// <summary>
    ///     Cancels a subscription by setting its Active property to false
    /// </summary>
    [HttpPatch("{subscriptionId:guid}/cancel")]
    public async Task<SubscriptionViewModel> CancelSubscription(Guid subscriptionId, CancellationToken cancellationToken)
    {
        var cancelledSubscription = await subscriptionService.CancelSubscription(User.GetAuth0IdFromToken(), subscriptionId, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(cancelledSubscription);
    }

    /// <summary>
    ///     Renews an existing subscription by extending its DueDate and sets Active status to true
    /// </summary>
    [HttpPatch("{subscriptionId:guid}/renew")]
    public async Task<SubscriptionViewModel> RenewSubscription(Guid subscriptionId, [FromQuery] int monthsToRenew,CancellationToken cancellationToken)
    {
        var renew = await subscriptionService.RenewSubscription(subscriptionId, monthsToRenew, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(renew);
    }

    /// <summary>
    ///     Retrieves a list of upcoming bills for a specific user
    /// </summary>
    [HttpGet("bills/users")]
    public async Task<List<SubscriptionViewModel>> GetUpcomingBills(CancellationToken cancellationToken)
    {
        var getUpcomingBills = await subscriptionService.GetUpcomingBills(User.GetAuth0IdFromToken(), cancellationToken);
        return mapper.Map<List<SubscriptionViewModel>>(getUpcomingBills);
    }
}
