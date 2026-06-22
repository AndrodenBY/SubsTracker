using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Polly.Registry;
using SubsTracker.API.Auth.IdentityProvider;
using SubsTracker.API.Auth.Session;
using SubsTracker.API.Resilience;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Helpers;

public class UserOrchestrator(IAuth0Service auth0Service, IUserService userService, ResiliencePipelineProvider<string> pipelineProvider)
{
    public async Task<UserDto> GetCurrentProfile(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? throw new UnauthorizedAccessException("NameIdentifier claim is missing");

        var pipeline = pipelineProvider.GetPipeline(ResilienceConstants.OrchestratorPipeline);
        
        var currentUser = await pipeline.ExecuteAsync(
            static async (state, token) =>
            {
                var (primaryId, user) = state;

                if (Guid.TryParse(primaryId, out var internalId))
                    return await user.GetById(internalId, token);

                return await user.GetByIdentityId(primaryId, token);
            },
            (primaryId: nameIdentifier, userService), cancellationToken);
        
        return currentUser ?? throw new UnauthorizedAccessException($"User with identifier '{nameIdentifier}' does not exist");
    }
    
    public async Task<UserDto> FullUserUpdate(
        HttpContext httpContext,
        Guid userId,
        string identityId,
        UpdateUserDto updateDto,
        CancellationToken cancellationToken)
    {
        var pipeline = pipelineProvider.GetPipeline(ResilienceConstants.OrchestratorPipeline);

        return await pipeline.ExecuteAsync(async (state, token) =>
        {
            await state.auth0Service.UpdateUserProfile(state.identityId, state.updateDto, token);
            var updatedUser = await state.userService.Update(state.userId, state.updateDto, token);
            await state.httpContext.RefreshSession(state.userService, token);

            return updatedUser;

        }, (auth0Service, userService, userId, identityId, updateDto, httpContext), cancellationToken);
    }
    
    public async Task FullUserDelete(
        HttpContext httpContext,
        Guid userId,
        string identityId,
        CancellationToken cancellationToken)
    {
        var pipeline = pipelineProvider.GetPipeline(ResilienceConstants.OrchestratorPipeline);

        await pipeline.ExecuteAsync(async (state, token) =>
        {
            await state.auth0Service.DeleteUserProfile(state.identityId, token);
            await userService.Delete(userId, token);
            await state.httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
        }, (auth0Service, userService, userId, identityId, httpContext), cancellationToken);
    }
}
