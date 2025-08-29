using Microsoft.AspNetCore.Mvc;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.Domain.Interfaces;

namespace SubsTracker.API.Interfaces;

public interface IController<TViewModel, TCreateViewModel, TUpdateViewModel>
    where TViewModel : class
    where TCreateViewModel : class
    where TUpdateViewModel : class
{
    Task<TViewModel> GetById(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<TViewModel>> GetAll(CancellationToken cancellationToken);
    Task<TViewModel> Create(TCreateViewModel createDto, CancellationToken cancellationToken); 
    Task<TViewModel> Update(Guid id, TUpdateViewModel updateDto, CancellationToken cancellationToken);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken);
}