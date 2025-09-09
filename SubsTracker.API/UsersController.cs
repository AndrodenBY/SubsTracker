using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.ViewModel.User;
using SubsTracker.API.ViewModel.User.Create;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(
    IUserService service, 
    IMapper mapper,
    IValidator<CreateUserDto> createValidator,
    IValidator<UpdateUserDto> updateValidator
    ) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<UserViewModel> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<UserViewModel>(getById);
    }
    
    [HttpGet("email/{email}")]
    public async Task<UserViewModel> GetByEmail(string email, CancellationToken cancellationToken)
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
        await createValidator.ValidateAndThrowAsync(createDto, cancellationToken);
        var create = await service.Create(createDto, cancellationToken);
        return mapper.Map<UserViewModel>(create);
    }
    
    [HttpPut("{id:guid}")]
    public async Task<UserViewModel> Update(Guid id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    { 
        await updateValidator.ValidateAndThrowAsync(updateDto, cancellationToken);
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<UserViewModel>(update);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        await service.Delete(id, cancellationToken);
    }
}
