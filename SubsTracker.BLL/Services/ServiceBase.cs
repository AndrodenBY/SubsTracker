using System.Linq.Expressions;
using AutoMapper;
using SubsTracker.BLL.DTOs;
using SubsTracker.Domain;

namespace SubsTracker.BLL.Services;

public class ServiceBase<TEntity, TDto, TCreateDto, TUpdateDto> : IService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : class, IBaseModel
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly IMapper _mapper;

    protected ServiceBase(IRepository<TEntity> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TDto>?> GetAll(CancellationToken cancellationToken)
    {
        var entities = await _repository.GetAll(cancellationToken);
        return _mapper.Map<IEnumerable<TDto>>(entities);
    }
    
    public async Task<TDto?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetById(id, cancellationToken);
        return _mapper.Map<TDto>(entity);
    }
    
    public async Task<TDto> Create(TCreateDto createDto, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<TEntity>(createDto);
        var createdEntity = await _repository.Create(entity, cancellationToken);
        return _mapper.Map<TDto>(createdEntity);
    }
    
    public async Task<TDto> Update(TUpdateDto updateDto, CancellationToken cancellationToken)
    {
        var existingEntity = await _repository.GetById((updateDto as IBaseDto).Id, cancellationToken);
        if (existingEntity == null) return null;
        
        _mapper.Map(updateDto, existingEntity);
        var updatedEntity = await _repository.Update(existingEntity, cancellationToken);
        
        return _mapper.Map<TDto>(updatedEntity);
    }
    
    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        return await _repository.Delete(id, cancellationToken);
    }
    
    protected async Task<TEntity?> FindByCondition(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        return await _repository.FindByCondition(predicate, cancellationToken);
    }
}