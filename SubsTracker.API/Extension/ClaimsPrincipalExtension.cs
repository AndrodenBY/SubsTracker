using System.Security.Claims;

namespace SubsTracker.API.Extension;

public static class ClaimsPrincipalExtension
{
    public static Guid GetInternalId(this ClaimsPrincipal principal)
    {
        var internalId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(internalId) || !Guid.TryParse(internalId, out var userId))
        {
            throw new UnauthorizedAccessException("Internal User ID is missing from session");
        }

        return userId;
    }

    public static string GetIdentityId(this ClaimsPrincipal principal)
    {
        var identityId = principal.FindFirstValue("identity_id");
        
        if (!string.IsNullOrEmpty(identityId))
        {
            return identityId;
        }

        var nameId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        
        return nameId
               ?? throw new UnauthorizedAccessException("Identity identifier is missing");
    }
    
    
}
