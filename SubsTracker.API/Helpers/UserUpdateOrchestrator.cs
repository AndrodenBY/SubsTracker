using SubsTracker.API.Auth0;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;
using Polly.Registry;
using SubsTracker.API.Resilience;

namespace SubsTracker.API.Helpers;

public class UserUpdateOrchestrator(IAuth0Service auth0Service, IUserService userService, ResiliencePipelineProvider<string> provider)
{
    public async Task<UserDto> FullUserUpdate(string auth0Id, UpdateUserDto updateDto, CancellationToken cancellationToken)
    {
        var pipeline = provider.GetPipeline(ResilienceConstants.OrchestratorPipeline);
        
        return await pipeline.ExecuteAsync(async (state, token) =>
            {
                await state.auth0Service.UpdateUserProfile(state.auth0Id, state.updateDto, token);
                return await state.userService.Update(state.auth0Id, state.updateDto, token);
            }, 
            (auth0Service, userService, auth0Id, updateDto), 
            cancellationToken);
    }
}
