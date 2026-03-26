using System.Security.Claims;
using SubsTracker.API.Constants;

namespace SubsTracker.API.Extension;

public static class ClaimsPrincipalExtension
{
    public static Guid GetInternalId(this ClaimsPrincipal principal)
    {
        var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (string.IsNullOrEmpty(nameIdentifier) || !Guid.TryParse(nameIdentifier, out var userId))
        {
            throw new UnauthorizedAccessException("Internal User ID is missing from session");
        }

        return userId;
    }

    public static string GetIdentityId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimsConstants.IdentityId)
               ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
               ?? principal.FindFirstValue(ClaimsConstants.Sub)
               ?? throw new UnauthorizedAccessException("Identity identifier is missing");
    }
}
