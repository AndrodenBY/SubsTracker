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
) : Service<UserModel, UserDto, CreateUserDto, UpdateUserDto, UserFilterDto>(repository, mapper, cacheService),
    IUserService
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
            throw new InvalidOperationException($"User with email {userExists.Email} already exists");

        if (existingUser is null)
        {
            var newUser = mapper.Map<UserModel>(createDto);
            newUser.Auth0Id = auth0Id; 
            var createdUser = await userRepository.Create(newUser, cancellationToken);
            return mapper.Map<UserDto>(createdUser);
        }
        
        if (string.IsNullOrEmpty(existingUser.Auth0Id))
        {
            existingUser.Auth0Id = auth0Id;
            await userRepository.Update(existingUser, cancellationToken);
        }
    
        return mapper.Map<UserDto>(existingUser);
    }

    public async Task<UserDto> Update(string auth0Id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                   ?? throw new NotFoundException($"User with id {auth0Id} not found");
        
        Mapper.Map(updateDto, user);
        var updatedEntity = await userRepository.Update(user, cancellationToken);
    
        return Mapper.Map<UserDto>(updatedEntity);
    }

    public async Task<bool> Delete(string auth0Id, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                   ?? throw new NotFoundException($"User with id {auth0Id} not found");
        
        return await userRepository.Delete(user, cancellationToken);
    }
}
