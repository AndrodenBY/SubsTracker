using Microsoft.AspNetCore.Mvc;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Interfaces;
using System.Net;
using AutoMapper;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Controller<TEntity, TDto, TCreateDto, TUpdateDto, TViewModel>
    (IService<TEntity, TDto, TCreateDto, TUpdateDto> service, IMapper mapper)
    : ControllerBase
    where TEntity : class, IBaseModel
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
    where TViewModel : class
{
    [HttpGet("{id:guid}")]
    public async Task<TViewModel?> GetById(Guid id, CancellationToken cancellationToken)
    {
        var getById = await service.GetById(id, cancellationToken);
        return mapper.Map<TViewModel>(getById);
    }
    
    [HttpGet]
    public async Task<IEnumerable<TViewModel>> GetAll(CancellationToken cancellationToken)
    {
        var getAll = await service.GetAll(cancellationToken);
        return mapper.Map<IEnumerable<TViewModel>>(getAll);
    }

    [HttpPost]
    public async Task<TViewModel> Create(TCreateDto createDto, CancellationToken cancellationToken)
    {
        var create = await service.Create(createDto, cancellationToken);
        return mapper.Map<TViewModel>(create);
    }

    [HttpPut("{id:guid}")]
    public async Task<TViewModel> Update(Guid id, TUpdateDto updateDto, CancellationToken cancellationToken)
    { 
        var update = await service.Update(id, updateDto, cancellationToken);
        return mapper.Map<TViewModel>(update);
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        var isDeleted = await service.Delete(id, cancellationToken);
        return isDeleted;
    }
}
