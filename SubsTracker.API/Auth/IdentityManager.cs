using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SubsTracker.API.Constants;
using SubsTracker.API.Extension;
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

        var identityId = claimsPrincipal.FindFirstValue(ClaimsConstants.Sub) ?? claimsPrincipal.FindFirstValue(ClaimsConstants.IdentityId);
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
            new (ClaimsConstants.IdentityId, identityId),
            new (ClaimsConstants.RefreshedAt, DateTimeOffset.UtcNow.ToString("O")),
            new (ClaimsConstants.AuthMethod, ClaimsConstants.MethodSessionSync) 
        };
        
        var internalIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(internalIdentity);
    }

    public static async Task ValidateIdentity(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var user = httpContext.User;

        var tokenIdentityId = user.GetIdentityId();
        var cookieIdentityId = user.FindFirstValue(ClaimsConstants.IdentityId);

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
