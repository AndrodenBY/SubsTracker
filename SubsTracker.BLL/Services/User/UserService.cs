using AutoMapper;
using DispatchR;
using SubsTracker.BLL.Handlers.Signals.User;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Interfaces;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;
using UserDto = SubsTracker.BLL.DTOs.User.UserDto;
using CreateUserDto = SubsTracker.BLL.DTOs.User.Create.CreateUserDto;
using UpdateUserDto = SubsTracker.BLL.DTOs.User.Update.UpdateUserDto;
using UserModel = SubsTracker.DAL.Models.User.User;

namespace SubsTracker.BLL.Services.User;

public class UserService(
    IUserRepository userRepository,
    IMapper mapper,
    ICacheService cacheService,
    IMediator mediator) 
    : Service<UserModel, UserDto, CreateUserDto, UpdateUserDto, UserFilterDto>(userRepository, mapper, cacheService),
    IUserService
{
    public async Task<PaginatedList<UserDto>> GetAll(UserFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken)
    {
        var predicate = UserFilterHelper.CreatePredicate(filter);
        return await base.GetAll(predicate, paginationParameters, cancellationToken);
    }

    public async Task<UserDto?> GetByAuth0Id(string auth0Id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<UserModel>(auth0Id);

        async Task<UserDto?> GetUser()
        {
            var user = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                       ?? throw new UnknownIdentifierException($"User with {auth0Id} not found");
            return Mapper.Map<UserDto>(user);
        }

        return await CacheService.CacheDataWithLock(cacheKey, GetUser, cancellationToken);
    }
    
    public async Task<UserDto> Create(string auth0Id, CreateUserDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByPredicate(user => user.Email == createDto.Email, cancellationToken);

        if (existingUser is null)
        {
            var newUser = Mapper.Map<UserModel>(createDto);
            newUser.Auth0Id = auth0Id; 
            var createdUser = await userRepository.Create(newUser, cancellationToken);
            return Mapper.Map<UserDto>(createdUser);
        }
        
        if (string.IsNullOrEmpty(existingUser.Auth0Id))
        {
            existingUser.Auth0Id = auth0Id;
            await userRepository.Update(existingUser, cancellationToken);
        }

        await mediator.Publish(new UserCreatedSignal(existingUser.Auth0Id), cancellationToken);
        return Mapper.Map<UserDto>(existingUser);
    }

    public async Task<UserDto> Update(string auth0Id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                   ?? throw new UnknownIdentifierException($"User with id {auth0Id} not found");
        
        Mapper.Map(updateDto, existingUser);
        var updatedEntity = await userRepository.Update(existingUser, cancellationToken);
    
        
        await mediator.Publish(new UserUpdatedSignal(updatedEntity.Auth0Id), cancellationToken);
        return Mapper.Map<UserDto>(updatedEntity);
    }

    public async Task<bool> Delete(string auth0Id, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByAuth0Id(auth0Id, cancellationToken)
                   ?? throw new UnknownIdentifierException($"User with id {auth0Id} not found");

        await mediator.Publish(new UserDeletedSignal(existingUser.Auth0Id), cancellationToken);
        return await userRepository.Delete(existingUser, cancellationToken);
    }
}
