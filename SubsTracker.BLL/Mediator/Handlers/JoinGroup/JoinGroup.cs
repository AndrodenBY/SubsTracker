using DispatchR.Abstractions.Send;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;

namespace SubsTracker.BLL.Mediator.Handlers.JoinGroup;

public record JoinGroup(CreateMemberDto CreateDto) : IRequest<JoinGroup, ValueTask<MemberDto>>;
