using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Auth;

public static class IdentityManager
{
    public static async Task<ClaimsPrincipal?> TransformRequestClaims(
        HttpContext context, 
        IUserService userService, 
        CancellationToken cancellationToken)
    {
        var claimsPrincipal = context.User;
        if (claimsPrincipal.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

        var identityId = claimsPrincipal.FindFirstValue("sub") ?? claimsPrincipal.FindFirstValue("identity_id");
        if (string.IsNullOrEmpty(identityId))
        {
            return null;
        }
        
        var userDto = await userService.GetByIdentityId(identityId, cancellationToken);
        if (userDto is null)
        {
            return null;
        }
    
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new ("identity_id", identityId),
            new ("refreshed_at", DateTimeOffset.UtcNow.ToString("O")),
            new ("auth_method", "session_sync") 
        };
        
        var internalIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(internalIdentity);
    }

    public static async Task ValidateIdentity(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var user = httpContext.User;
        
        var tokenIdentityId = user.FindFirstValue("sub");
        var cookieIdentityId = user.FindFirstValue("identity_id");

        if (string.IsNullOrEmpty(tokenIdentityId) || string.IsNullOrEmpty(cookieIdentityId))
        {
            return;
        }
        
        if (tokenIdentityId != cookieIdentityId)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
