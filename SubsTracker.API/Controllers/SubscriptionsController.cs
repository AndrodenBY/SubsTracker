using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.ViewModel.Subscription;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Interfaces.Subscription;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;

namespace SubsTracker.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController(
    ISubscriptionService service,
    IMapper mapper
) : ControllerBase
{
    /// <summary>
    ///     Retrieves a subscription by its ID
    /// </summary>
    /// <param name="id">The ID of the subscription</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     GET /api/subscriptions/{id}
    /// </remarks>
    /// <returns>A subscription view model</returns>
    [HttpGet("{id:guid}")]
    public async Task<SubscriptionViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetUserInfoById(id, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(getById);
    }

    /// <summary>
    ///     Retrieves all subscriptions with optional filtering
    /// </summary>
    /// <param name="filterDto">Filter parameters for the subscriptions</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     GET /api/subscriptions?userId={userId}
    /// </remarks>
    /// <returns>A list of subscription view models</returns>
    [HttpGet]
    public async Task<List<SubscriptionViewModel>> GetAll([FromQuery] SubscriptionFilterDto? filterDto,
        CancellationToken cancellationToken)
    {
        var entities = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<List<SubscriptionViewModel>>(entities);
    }

    /// <summary>
    ///     Creates a new subscription for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user to create the subscription for</param>
    /// <param name="createDto">The subscription data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     POST /api/subscriptions/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///     "name": "Netflix",
    ///     "price": 10.99,
    ///     "dueDate": "2025-01-01",
    ///     "type": 7,
    ///     "content": 1
    ///     }
    /// </remarks>
    /// <returns>The created subscription view model</returns>
    /// <exception cref="NotFoundException">Thrown if the user is not found</exception>
    [HttpPost("{userId:guid}")]
    public async Task<SubscriptionViewModel> Create(Guid userId, [FromBody] CreateSubscriptionDto createDto,
        CancellationToken cancellationToken)
    {
        var create = await service.Create(userId, createDto, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(create);
    }

    /// <summary>
    ///     Updates an existing subscription
    /// </summary>
    /// <param name="id">The ID of the user owning subscription</param>
    /// <param name="updateDto">The updated subscription data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     PUT /api/subscriptions/3fa85f64-5717-4562-b3fc-2c963f66afa6
    ///     {
    ///     "name": "Netflix Premium",
    ///     "price": 15.99
    ///     }
    /// </remarks>
    /// <returns>The updated subscription view model</returns>
    /// <exception cref="NotFoundException">Thrown if the subscription is not found, or the user with such a subscription</exception>
    [HttpPut("{id:guid}")]
    public async Task<SubscriptionViewModel> Update(Guid id, [FromBody] UpdateSubscriptionDto updateDto,
        CancellationToken cancellationToken)
    {
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(update);
    }

    /// <summary>
    ///     Cancels a subscription by setting its Active property to false
    /// </summary>
    /// <param name="userId">The ID of the user with subscriptions</param>
    /// <param name="subscriptionId">The ID of the subscription to cancel</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     PUT /api/subscriptions/{subscriptionId}/cancel?userId={userId}
    /// </remarks>
    /// <returns>The cancelled subscription view model</returns>
    /// <exception cref="NotFoundException">Thrown if the subscription is not found</exception>
    [HttpPatch("{subscriptionId:guid}/cancel")]
    public async Task<SubscriptionViewModel> CancelSubscription([FromQuery] Guid userId, Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var cancelledSubscription = await service.CancelSubscription(userId, subscriptionId, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(cancelledSubscription);
    }

    /// <summary>
    ///     Renews an existing subscription by extending its DueDate and sets Active status to true
    /// </summary>
    /// <param name="subscriptionId">The ID of the subscription to renew</param>
    /// <param name="monthsToRenew">The number of months to add to the current due date (must be greater than 0)</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     PATCH /api/subscriptions/{subscriptionId}/renew?monthsToRenew=3
    /// </remarks>
    /// <returns>The updated subscription details, including the new DueDate</returns>
    /// <exception cref="NotFoundException">Thrown if the subscription is not found</exception>
    /// <exception cref="ValidationException">Thrown if the number of months to renew is zero or negative</exception>
    [HttpPatch("{subscriptionId:guid}/renew")]
    public async Task<SubscriptionViewModel> RenewSubscription(Guid subscriptionId, [FromQuery] int monthsToRenew,
        CancellationToken cancellationToken)
    {
        var renew = await service.RenewSubscription(subscriptionId, monthsToRenew, cancellationToken);
        return mapper.Map<SubscriptionViewModel>(renew);
    }

    /// <summary>
    ///     Retrieves a list of upcoming bills for a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <remarks>
    ///     Sample request:
    ///     GET /api/subscriptions/bills/users/{userId}
    /// </remarks>
    /// <returns>A list of subscriptions due in the next 7 days</returns>
    /// <exception cref="NotFoundException">Thrown if the user is not found</exception>
    [HttpGet("bills/users/{userId:guid}")]
    public async Task<List<SubscriptionViewModel>> GetUpcomingBills(Guid userId, CancellationToken cancellationToken)
    {
        var getUpcomingBills = await service.GetUpcomingBills(userId, cancellationToken);
        return mapper.Map<List<SubscriptionViewModel>>(getUpcomingBills);
    }
}
