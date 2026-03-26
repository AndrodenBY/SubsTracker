using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Filter;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces;

public interface IMemberService
{
    Task<MemberDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken);
    Task<MemberDto?> GetById(Guid id, CancellationToken cancellationToken);
    Task<PaginatedList<MemberDto>> GetAll(MemberFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
    Task<MemberDto> Create(CreateMemberDto createDto, CancellationToken cancellationToken);
    Task<MemberDto> JoinGroup(CreateMemberDto createDto, CancellationToken cancellationToken);
    Task<MemberDto> ChangeRole(Guid memberId, CancellationToken cancellationToken);
    Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken);
    
}
