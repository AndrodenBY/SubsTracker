using System.Security.Claims;

namespace SubsTracker.API.Extension;

public static class ClaimsPrincipalExtension
{
    public static Guid GetInternalId(this ClaimsPrincipal principal)
    {
        var internalId = principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrEmpty(internalId) || !Guid.TryParse(internalId, out var userId))
        {
            throw new UnauthorizedAccessException("Internal User ID is missing from session");
        }

        return userId;
    }

    public static string GetIdentityId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier) 
               ?? throw new UnauthorizedAccessException("Identity identifier is missing");
    }
}
