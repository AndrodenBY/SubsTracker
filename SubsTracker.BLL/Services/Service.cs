using System.Linq.Expressions;
using AutoMapper;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.Cache;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Exceptions;

namespace SubsTracker.BLL.Services;

public class Service<TEntity, TDto, TCreateDto, TUpdateDto, TFilterDto>(
    IRepository<TEntity> repository,
    IMapper mapper,
    ICacheService cacheService
    ) : IService<TEntity, TDto, TCreateDto, TUpdateDto, TFilterDto>
    where TEntity : class, IBaseModel
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TFilterDto : class
{
    protected IMapper Mapper => mapper;

    protected IRepository<TEntity> Repository => repository;
    
    protected ICacheService CacheService => cacheService;

    public virtual async Task<List<TDto>> GetAll(
        Expression<Func<TEntity, bool>>? predicate, CancellationToken cancellationToken)
    {
        var entities = await repository.GetAll(predicate, cancellationToken);
        return Mapper.Map<List<TDto>>(entities);
    }

    public virtual async Task<TDto?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var cacheKey = $"{id}_{typeof(TEntity).Name}";
        var cachedDto = CacheService.GetData<TDto>(cacheKey);
        if (cachedDto is not null)
        {
            return cachedDto;
        }
        
        var entity = await repository.GetById(id, cancellationToken);
        var mappedEntity =  Mapper.Map<TDto>(entity);
        
        CacheService.SetData(cacheKey, mappedEntity, TimeSpan.FromMinutes(3));
        return mappedEntity;
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
                             ?? throw new NotFoundException($"Entity with id {updateId} not found");

        Mapper.Map(updateDto, existingEntity);
        var updatedEntity = await repository.Update(existingEntity, cancellationToken);

        return Mapper.Map<TDto>(updatedEntity);
    }

    public virtual async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingEntity = await repository.GetById(id, cancellationToken)
                             ?? throw new NotFoundException($"Entity with id {id} not found");

        return await repository.Delete(existingEntity, cancellationToken);
    }

    public virtual async Task<TDto?> GetByPredicate(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByPredicate(predicate, cancellationToken);

        return Mapper.Map<TDto>(entity);
    }
}
