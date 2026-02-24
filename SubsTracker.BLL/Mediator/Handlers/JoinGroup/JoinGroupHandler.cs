using AutoMapper;
using DispatchR;
using DispatchR.Abstractions.Send;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.Helpers.Policy;
using SubsTracker.BLL.Mediator.Signals;
using SubsTracker.DAL.Entities;
using SubsTracker.DAL.Interfaces.Repositories;

namespace SubsTracker.BLL.Mediator.Handlers.JoinGroup;

public class JoinGroupHandler(
    IMemberRepository memberRepository,
    IMemberPolicyChecker policyChecker,
    IMediator mediator,
    IMapper mapper) 
    : IRequestHandler<JoinGroup, ValueTask<MemberDto>>
{
    public async ValueTask<MemberDto> Handle(JoinGroup request, CancellationToken cancellationToken)
    {
        await policyChecker.EnsureCanJoinGroup(request.CreateDto, cancellationToken);
        
        var entity = mapper.Map<MemberEntity>(request.CreateDto);
        var createdEntity = await memberRepository.Create(entity, cancellationToken);
        var memberDto = mapper.Map<MemberDto>(createdEntity);
        
        await mediator.Publish(new MemberSignals.Joined(memberDto.UserId, memberDto.GroupId), cancellationToken);
        return memberDto;
    }
}
