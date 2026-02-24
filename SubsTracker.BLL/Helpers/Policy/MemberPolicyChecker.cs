using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.Domain.Exceptions;

namespace SubsTracker.BLL.Helpers.Policy;

public class MemberPolicyChecker(
    IUserRepository userRepository,
    IGroupRepository groupRepository,
    IMemberRepository memberRepository) 
    : IMemberPolicyChecker
{
    public async Task EnsureCanJoinGroup(CreateMemberDto createDto, CancellationToken cancellationToken)
    {
        _ = await userRepository.GetById(createDto.UserId, cancellationToken)
            ?? throw new UnknownIdentifierException($"User with id {createDto.UserId} not found");

        _ = await groupRepository.GetById(createDto.GroupId, cancellationToken)
            ?? throw new UnknownIdentifierException($"Group with id {createDto.GroupId} not found");

        var exists = await memberRepository.GetByPredicate(
            member => member.UserId == createDto.UserId && member.GroupId == createDto.GroupId, cancellationToken);

        if (exists is not null)
        {
            throw new PolicyViolationException("User is already a member of this group");
        }
    }
}
