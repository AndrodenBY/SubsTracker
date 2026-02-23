using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.BLL.Interfaces;

public interface IMemberService : IService<MemberEntity, MemberDto, CreateMemberDto, UpdateMemberDto, MemberFilterDto>
{
    Task<MemberDto?> GetFullInfoById(Guid id, CancellationToken cancellationToken);
    Task<PaginatedList<MemberDto>> GetAll(MemberFilterDto? filter, PaginationParameters? paginationParameters, CancellationToken cancellationToken);
    Task<MemberDto> JoinGroup(CreateMemberDto createDto, CancellationToken cancellationToken);
    Task<bool> LeaveGroup(Guid groupId, Guid userId, CancellationToken cancellationToken);
    Task<MemberDto> ChangeRole(Guid memberId, CancellationToken cancellationToken);
}
