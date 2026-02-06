using System.Security.Claims;

namespace SubsTracker.API.Extension;

public static class ClaimsPrincipalExtension
{
    public static string GetAuth0IdFromToken(this ClaimsPrincipal claimsPrincipal)
    {
        var auth0Id = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Auth0Id is missing from token");

        return auth0Id;
    }
}
