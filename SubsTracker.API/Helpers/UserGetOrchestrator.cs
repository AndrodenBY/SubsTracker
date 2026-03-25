using System.Security.Claims;
using Polly.Registry;
using SubsTracker.API.Resilience;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Helpers;

public class UserGetOrchestrator(IUserService userService,  ResiliencePipelineProvider<string> pipelineProvider)
{
    public async Task<UserDto> GetCurrentProfile(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        var nameIdentifier = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? throw new UnauthorizedAccessException("NameIdentifier claim is missing");

        var pipeline = pipelineProvider.GetPipeline(ResilienceConstants.OrchestratorPipeline);
        
        var currentUser = await pipeline.ExecuteAsync(
            static async (state, token) =>
            {
                var (primaryId, userService) = state;

                if (Guid.TryParse(primaryId, out var internalId))
                    return await userService.GetById(internalId, token);

                return await userService.GetByIdentityId(primaryId, token);
            },
            (primaryId: nameIdentifier, userService), cancellationToken);
        
        return currentUser ?? throw new UnauthorizedAccessException($"User with identifier '{nameIdentifier}' does not exist");
    }
}
