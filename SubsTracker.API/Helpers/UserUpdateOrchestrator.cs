using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;
using Polly.Registry;
using SubsTracker.API.Auth.IdentityProvider;
using SubsTracker.API.Auth.Session;
using SubsTracker.API.Resilience;

namespace SubsTracker.API.Helpers;

public class UserUpdateOrchestrator(IAuth0Service auth0Service, IUserService userService, ResiliencePipelineProvider<string> provider)
{
    public async Task<UserDto> FullUserUpdate(
        HttpContext httpContext,
        Guid userId,
        string identityId,
        UpdateUserDto updateDto,
        CancellationToken cancellationToken)
    {
        var pipeline = provider.GetPipeline(ResilienceConstants.OrchestratorPipeline);

        return await pipeline.ExecuteAsync(async (state, token) =>
        {
            await state.auth0Service.UpdateUserProfile(state.identityId, state.updateDto, token);
            var updatedUser = await state.userService.Update(state.userId, state.updateDto, token);
            await state.httpContext.RefreshSession(state.userService, token);

            return updatedUser;

        }, (auth0Service, userService, userId, identityId, updateDto, httpContext), cancellationToken);
    }
}
