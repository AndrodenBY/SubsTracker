using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.ViewModel.Subscription;
using SubsTracker.API.ViewModel.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserGroupsController(
    IUserGroupService service, 
    IMapper mapper,
    IValidator<CreateUserGroupDto> createValidator,
    IValidator<UpdateUserGroupDto> updateValidator
    ) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<UserGroupViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<UserGroupViewModel>(getById);
    }
    
    [HttpGet]
    public async Task<IEnumerable<UserGroupViewModel>> GetAll(CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(cancellationToken);
        return mapper.Map<IEnumerable<UserGroupViewModel>>(getAll);
    }

    [HttpPost]
    public async Task<UserGroupViewModel> Create([FromBody] CreateUserGroupDto createDto, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(createDto, cancellationToken);
        var create = await service.Create(createDto, cancellationToken);
        return mapper.Map<UserGroupViewModel>(create);
    }

    [HttpPut("{id:guid}")]
    public async Task<UserGroupViewModel> Update(Guid id, [FromBody] UpdateUserGroupDto updateDto, CancellationToken cancellationToken)
    { 
        await updateValidator.ValidateAndThrowAsync(updateDto, cancellationToken);
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<UserGroupViewModel>(update);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    { 
        await service.Delete(id, cancellationToken);
    }

    [HttpDelete("leave")]
    public async Task LeaveGroup([FromQuery] Guid groupId, [FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        await service.LeaveGroup(groupId, userId, cancellationToken);
    }

    [HttpPost("join")]
    public async Task JoinGroup([FromBody] CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        await service.JoinGroup(createDto, cancellationToken);
    }

    [HttpPost("share")]
    public async Task<UserGroupViewModel> ShareSubscription([FromQuery] Guid groupId, [FromQuery] Guid subscriptionId, CancellationToken cancellationToken)
    {
        var updatedGroup = await service.ShareSubscription(groupId, subscriptionId, cancellationToken);
        return mapper.Map<UserGroupViewModel>(updatedGroup);
    }
    
    [HttpPost("unshare")]
    public async Task<UserGroupViewModel> UnshareSubscription([FromQuery] Guid groupId, [FromQuery] Guid subscriptionId, CancellationToken cancellationToken)
    {
        var updatedGroup = await service.UnshareSubscription(groupId, subscriptionId, cancellationToken);
        return mapper.Map<UserGroupViewModel>(updatedGroup);
    }
}
