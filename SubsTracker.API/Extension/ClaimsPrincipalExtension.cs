using System.Security.Claims;

namespace SubsTracker.API.Extension;

public static class ClaimsPrincipalExtension
{
    public static string GetIdentityIdFromToken(this ClaimsPrincipal claimsPrincipal)
    {
        var identityId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("IdentityId is missing from token");

        return identityId;
    }
}
