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
    : Service<UserEntity, UserDto, CreateUserDto, UpdateUserDto, UserFilterDto>(userRepository, mapper, cacheService),
    IUserService
{
    public async Task<PaginatedList<UserDto>> GetAll(UserFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken)
    {
        var predicate = UserFilterHelper.CreatePredicate(filter);
        return await base.GetAll(predicate, paginationParameters, cancellationToken);
    }

    public async Task<UserDto?> GetByIdentityId(string identityId, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<UserEntity>(identityId);
        return await CacheService.CacheDataWithLock(cacheKey, GetUser, cancellationToken);
        
        async Task<UserDto?> GetUser()
        {
            var user = await userRepository.GetByIdentityId(identityId, cancellationToken)
                       ?? throw new UnknownIdentifierException($"User with {identityId} not found");
            return Mapper.Map<UserDto>(user);
        }
    }
    
    public async Task<UserDto> Create(string identityId, CreateUserDto createDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByPredicate(user => user.Email == createDto.Email, cancellationToken);

        if (existingUser is null)
        {
            var newUser = Mapper.Map<UserEntity>(createDto);
            newUser.IdentityId = identityId; 
            var createdUser = await userRepository.Create(newUser, cancellationToken);
            
            await mediator.Publish(new UserSignals.Created(createdUser.IdentityId), cancellationToken);
            return Mapper.Map<UserDto>(createdUser);
        }
        
        if (string.IsNullOrEmpty(existingUser.IdentityId))
        {
            existingUser.IdentityId = identityId;
            await userRepository.Update(existingUser, cancellationToken);
            
            await mediator.Publish(new UserSignals.Updated(existingUser.Id), cancellationToken);
        }
        
        return Mapper.Map<UserDto>(existingUser);
    }

    public new async Task<UserDto> Update(Guid id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetById(id, cancellationToken)
                   ?? throw new UnknownIdentifierException($"User with id {id} not found");
        
        Mapper.Map(updateDto, existingUser);
        var updatedEntity = await userRepository.Update(existingUser, cancellationToken);
        
        await mediator.Publish(new UserSignals.Updated(updatedEntity.Id), cancellationToken);
        return Mapper.Map<UserDto>(updatedEntity);
    }

    public new async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetById(id, cancellationToken)
                   ?? throw new UnknownIdentifierException($"User with id {id} not found");

        await mediator.Publish(new UserSignals.Deleted(existingUser.Id), cancellationToken);
        return await userRepository.Delete(existingUser, cancellationToken);
    }
}
