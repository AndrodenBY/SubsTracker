using System.Security.Claims;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Helpers;

public class UserGetOrchestrator(IUserService userService)
{
    public async Task<UserDto?> GetCurrentProfile(ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        var primaryId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? throw new UnauthorizedAccessException("NameIdentifier claim is missing");
        
        if(Guid.TryParse(primaryId, out var internalId))
        {
            return await userService.GetById(internalId, cancellationToken);
        }
        
        return await userService.GetByIdentityId(primaryId, cancellationToken);
    }
}
