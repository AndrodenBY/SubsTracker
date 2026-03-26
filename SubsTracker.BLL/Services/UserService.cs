using AutoMapper;
using DispatchR;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Pagination;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Filter;

namespace SubsTracker.BLL.Services;

public class UserService(
    IUserRepository userRepository,
    IMapper mapper,
    ICacheService cacheService,
    IMediator mediator) 
    : IUserService
{
    public async Task<PaginatedList<UserDto>> GetAll(UserFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken)
    {
        var expression = UserFilterHelper.CreatePredicate(filter);
        var pagedEntities = await userRepository.GetAll(expression, paginationParameters, cancellationToken);
        return mapper.Map<PaginatedList<UserDto>>(pagedEntities);
    }

    public async Task<UserDto?> GetByIdentityId(string identityId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdentityId(identityId, cancellationToken);

        if (user is null)
        {
            return null;
        }
        
        var userDto = mapper.Map<UserDto>(user);
        var guidCacheKey = RedisKeySetter.SetCacheKey<UserEntity>(userDto.Id);
        
        await cacheService.CacheDataWithLock(
            guidCacheKey, 
            () => Task.FromResult<UserDto?>(userDto), 
            cancellationToken);

        return userDto;
    }
    
    public async Task<UserDto> Create(string identityId, CreateUserDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByPredicate(user => user.Email == createDto.Email, cancellationToken);

        if (existingUser is null)
        {
            var newUser = mapper.Map<UserEntity>(createDto);
            newUser.IdentityId = identityId; 
            var createdUser = await userRepository.Create(newUser, cancellationToken);
            
            await mediator.Publish(new UserSignals.Created(createdUser.IdentityId), cancellationToken);
            return mapper.Map<UserDto>(createdUser);
        }
        
        if (string.IsNullOrEmpty(existingUser.IdentityId))
        {
            existingUser.IdentityId = identityId;
            await userRepository.Update(existingUser, cancellationToken);
            
            await mediator.Publish(new UserSignals.Updated(existingUser.Id), cancellationToken);
        }
        
        return mapper.Map<UserDto>(existingUser);
    }

    public async Task<UserDto> Update(Guid id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetById(id, cancellationToken)
                   ?? throw new UnknownIdentifierException($"User with id {id} not found");
        
        mapper.Map(updateDto, existingUser);
        var updatedEntity = await userRepository.Update(existingUser, cancellationToken);
        
        await mediator.Publish(new UserSignals.Updated(updatedEntity.Id), cancellationToken);
        return mapper.Map<UserDto>(updatedEntity);
    }

    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetById(id, cancellationToken)
                   ?? throw new UnknownIdentifierException($"User with id {id} not found");

        await mediator.Publish(new UserSignals.Deleted(existingUser.Id), cancellationToken);
        return await userRepository.Delete(existingUser, cancellationToken);
    }
}
