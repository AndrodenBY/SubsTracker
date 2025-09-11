using System.Linq.Expressions;
using AutoMapper;
using SubsTracker.BLL.Interfaces;
using SubsTracker.DAL.Interfaces;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Exceptions;

namespace SubsTracker.BLL.Services;

public class Service<TEntity, TDto, TCreateDto, TUpdateDto>(
    IRepository<TEntity> repository, 
    IMapper mapper
    ) : IService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : class, IBaseModel
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    public virtual async Task<IEnumerable<TDto>> GetAll(CancellationToken cancellationToken)
    {
        var entities = await repository.GetAll(cancellationToken);
        return mapper.Map<IEnumerable<TDto>>(entities);
    }
    
    public virtual async Task<TDto> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entity = await repository.GetById(id, cancellationToken) 
                     ?? throw new NotFoundException($"Entity with id {id} not found");
        return mapper.Map<TDto>(entity);
    }
    
    public virtual async Task<TDto> Create(TCreateDto createDto, CancellationToken cancellationToken)
    {
        var entity = mapper.Map<TEntity>(createDto);
        var createdEntity = await repository.Create(entity, cancellationToken);
        return mapper.Map<TDto>(createdEntity);
    }
    
    public virtual async Task<TDto> Update(Guid updateId, TUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var existingEntity = await repository.GetById(updateId, cancellationToken) 
                             ?? throw new NotFoundException($"Entity with id {updateId} not found");
        
        mapper.Map(updateDto, existingEntity);
        var updatedEntity = await repository.Update(existingEntity, cancellationToken);
        
        return mapper.Map<TDto>(updatedEntity);
    }
    
    public virtual async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var existingEntity = await repository.GetById(id, cancellationToken) 
                             ?? throw new NotFoundException($"Entity with id {id} not found");
        
        return await repository.Delete(existingEntity, cancellationToken);
    }
    
    public virtual async Task<TDto?> GetByPredicate(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByPredicate(predicate, cancellationToken)
                     ?? throw new NotFoundException($"Entity with predicate {predicate} not found");

        return mapper.Map<TDto>(entity);
    }
}
