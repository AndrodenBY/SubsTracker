using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService service, IMapper mapper) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<UserViewModel?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<UserViewModel>(getById);
    }
    
    [HttpGet]
    public async Task<UserViewModel?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await service.GetByEmail(email, cancellationToken);
        return mapper.Map<UserViewModel>(user);
    }
    
    [HttpGet]
    public async Task<IEnumerable<UserViewModel>> GetAll(CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(cancellationToken);
        return mapper.Map<IEnumerable<UserViewModel>>(getAll);
    }
    
    [HttpPost]
    public async Task<UserViewModel> Create(CreateUserDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(createDto, cancellationToken);
        return mapper.Map<UserViewModel>(create);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<UserViewModel> Update(Guid id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    { 
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<UserViewModel>(update);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var isDeleted = await service.Delete(id, cancellationToken);
        return isDeleted;
    }
}