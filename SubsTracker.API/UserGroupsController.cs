using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.ViewModel.User;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Controller(IService<UserGroup, UserGroupDto, CreateUserGroupDto, UpdateUserGroupDto> service, IMapper mapper)
    : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<UserGroupViewModel?> GetById(Guid id, CancellationToken cancellationToken)
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
    public async Task<UserGroupViewModel> Create(CreateUserGroupDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(createDto, cancellationToken);
        return mapper.Map<UserGroupViewModel>(create);
    }

    [HttpPut("{id:guid}")]
    public async Task<UserGroupViewModel> Update(Guid id, UpdateUserGroupDto updateDto, CancellationToken cancellationToken)
    { 
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<UserGroupViewModel>(update);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var isDeleted = await service.Delete(id, cancellationToken);
        return isDeleted;
    }
}