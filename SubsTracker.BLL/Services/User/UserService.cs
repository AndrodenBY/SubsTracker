using AutoMapper;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Filter;
using UserDto = SubsTracker.BLL.DTOs.User.UserDto;
using CreateUserDto = SubsTracker.BLL.DTOs.User.Create.CreateUserDto;
using UpdateUserDto = SubsTracker.BLL.DTOs.User.Update.UpdateUserDto;
using UserModel = SubsTracker.DAL.Models.User.User;
using InvalidOperationException = SubsTracker.Domain.Exceptions.InvalidOperationException;

namespace SubsTracker.BLL.Services.User;

public class UserService(
    IRepository<UserModel> repository,
    IMapper mapper,
    ICacheService cacheService
    ) : Service<UserModel, UserDto, CreateUserDto, UpdateUserDto, UserFilterDto>(repository, mapper, cacheService), IUserService
{
    public async Task<List<UserDto>> GetAll(UserFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = UserFilterHelper.CreatePredicate(filter);
        return await base.GetAll(predicate, cancellationToken);
    }

    public override async Task<UserDto> Create(CreateUserDto createDto, CancellationToken cancellationToken)
    {
        var userExists = await Repository.GetByPredicate(user => user.Email == createDto.Email, cancellationToken);
        if (userExists is not null)
        {
            throw new InvalidOperationException($"User with email {userExists.Email} already exists");
        }
        
        return await base.Create(createDto, cancellationToken);
    }

    public override async Task<UserDto> Update(Guid updateId, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var userExists = await Repository.GetById(updateId, cancellationToken)
                         ?? throw new InvalidOperationException($"Cannot update user with id {updateId}");

        return await base.Update(userExists.Id, updateDto, cancellationToken);
    }
}
