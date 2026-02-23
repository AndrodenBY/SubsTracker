using System.Linq.Expressions;
using AutoMapper;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Services;

public class Service<TEntity, TDto, TCreateDto, TUpdateDto, TFilterDto>(
    IRepository<TEntity> repository,
    IMapper mapper,
    ICacheService cacheService) 
    : IService<TEntity, TDto, TCreateDto, TUpdateDto, TFilterDto>
    where TEntity : class, IBaseEntity
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TFilterDto : class
{
    protected IMapper Mapper => mapper;

    private IRepository<TEntity> Repository => repository;

    protected ICacheService CacheService => cacheService;

    public virtual async Task<PaginatedList<TDto>> GetAll(
        Expression<Func<TEntity, bool>>? predicate,
        PaginationParameters? paginationParameters,
        CancellationToken cancellationToken)
    {
        var pagedEntities = await Repository.GetAll(predicate, paginationParameters, cancellationToken);
        return pagedEntities.MapToPage(entity => Mapper.Map<TDto>(entity));
    }

    public virtual async Task<TDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = RedisKeySetter.SetCacheKey<TEntity>(id);
        var result = await CacheService.CacheDataWithLock(cacheKey, GetEntity, cancellationToken)
                     ?? throw new UnknownIdentifierException($"Entity with {id} not found");
        
        return result;
        
        async Task<TDto?> GetEntity()
        {
            var entity = await repository.GetById(id, cancellationToken);
            return Mapper.Map<TDto>(entity);
        }
    }

    public virtual async Task<TDto> Create(TCreateDto createDto, CancellationToken cancellationToken)
    {
        var entity = Mapper.Map<TEntity>(createDto);
        var createdEntity = await repository.Create(entity, cancellationToken);
        return Mapper.Map<TDto>(createdEntity);
    }

    public virtual async Task<TDto> Update(Guid updateId, TUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var existingEntity = await repository.GetById(updateId, cancellationToken)
                             ?? throw new UnknownIdentifierException($"Entity with id {updateId} not found");

        Mapper.Map(updateDto, existingEntity);
        var updatedEntity = await repository.Update(existingEntity, cancellationToken);

        return Mapper.Map<TDto>(updatedEntity);
    }

    public virtual async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingEntity = await repository.GetById(id, cancellationToken)
                             ?? throw new UnknownIdentifierException($"Entity with id {id} not found");

        return await repository.Delete(existingEntity, cancellationToken);
    }

    public virtual async Task<TDto?> GetByPredicate(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByPredicate(expression, cancellationToken);

        return Mapper.Map<TDto>(entity);
    }
}
