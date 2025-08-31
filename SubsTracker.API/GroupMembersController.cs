using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Controller(IGroupMemberService service, IMapper mapper) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<GroupMemberViewModel?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<GroupMemberViewModel>(getById);
    }
    
    [HttpGet]
    public async Task<IEnumerable<GroupMemberViewModel>> GetAll(CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(cancellationToken);
        return mapper.Map<IEnumerable<GroupMemberViewModel>>(getAll);
    }

    [HttpPost]
    public async Task<GroupMemberViewModel> Create(CreateGroupMemberDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(createDto, cancellationToken);
        return mapper.Map<GroupMemberViewModel>(create);
    }

    [HttpPut("{id:guid}")]
    public async Task<GroupMemberViewModel> Update(Guid id, UpdateGroupMemberDto updateDto, CancellationToken cancellationToken)
    { 
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<GroupMemberViewModel>(update);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var isDeleted = await service.Delete(id, cancellationToken);
        return isDeleted;
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<bool> LeaveGroup(Guid groupId, Guid userId, GroupMemberDto memberDto,
        CancellationToken cancellationToken)
    {
        var hasLeft = await service.LeaveGroup(groupId, userId, memberDto, cancellationToken);
        return hasLeft;
    }
}