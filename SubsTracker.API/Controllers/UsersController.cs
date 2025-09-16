using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.ViewModel.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.Domain.Filter;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    IUserService service, 
    IMapper mapper
    ) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<UserViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<UserViewModel>(getById);
    }
    
    [HttpGet]
    public async Task<IEnumerable<UserViewModel>> GetAll([FromQuery] UserFilterDto? filterDto, CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(filterDto, cancellationToken);
        return mapper.Map<IEnumerable<UserViewModel>>(getAll);
    }
    
    [HttpPost]
    public async Task<UserViewModel> Create([FromBody] CreateUserDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(createDto, cancellationToken);
        return mapper.Map<UserViewModel>(create);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<UserViewModel> Update(Guid id, [FromBody] UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<UserViewModel>(update);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.Delete(id, cancellationToken);
    }
}
