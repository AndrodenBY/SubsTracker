using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Filter;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces;

public interface IUserService : IService<UserEntity, UserDto, CreateUserDto, UpdateUserDto, UserFilterDto>
{
    Task<PaginatedList<UserDto>> GetAll(UserFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
    Task<UserDto?> GetByIdentityId(string identityId, CancellationToken cancellationToken);
    Task<UserDto> Create(string identityId, CreateUserDto createDto, CancellationToken cancellationToken);
    new Task<UserDto> Update(Guid id, UpdateUserDto updateDto, CancellationToken cancellationToken);
    new Task<bool> Delete(Guid id, CancellationToken cancellationToken);
}
