using System.Security.Claims;

namespace SubsTracker.API.Extension;

public static class ClaimsPrincipalExtension
{
    public static Guid GetInternalId(this ClaimsPrincipal principal)
    {
        var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine("NameIdentifier for update request: " + nameIdentifier);
        if (string.IsNullOrEmpty(nameIdentifier) || !Guid.TryParse(nameIdentifier, out var userId))
        {
            throw new UnauthorizedAccessException("Internal User ID is missing from session");
        }

        return userId;
    }

    public static string GetIdentityId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("identity_id") 
               ?? principal.FindFirstValue("sub")
               ?? throw new UnauthorizedAccessException("Identity identifier is missing");
    }
    
    // public static string GetSessionRefreshedTime(this ClaimsPrincipal principal)
    // {
    //     return principal.FindFirstValue("refreshed_at") ?? "Initial Login";
    // }
}
