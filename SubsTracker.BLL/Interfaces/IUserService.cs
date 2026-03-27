using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces;

public interface IUserService
{
    Task<PaginatedList<UserDto>> GetAll(UserFilter? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
    Task<UserDto> GetById(Guid id, CancellationToken cancellationToken);
    Task<UserDto?> GetByIdentityId(string identityId, CancellationToken cancellationToken);
    Task<UserDto> Create(string identityId, CreateUserDto createDto, CancellationToken cancellationToken);
    Task<UserDto> Update(Guid id, UpdateUserDto updateDto, CancellationToken cancellationToken);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken);
}
