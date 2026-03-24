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
        var principal = context.User;
        if (principal.Identity?.IsAuthenticated is not true)
        {
            return null;
        }

        var identity = (ClaimsIdentity)principal.Identity;
        var nameIdentifier = identity.FindFirst(ClaimTypes.NameIdentifier);
        if (nameIdentifier is null || Guid.TryParse(nameIdentifier.Value, out _))
        {
            return null;
        }

        var userDto = await userService.GetByIdentityId(nameIdentifier.Value, cancellationToken);
        if (userDto is null)
        {
            return null;
        }
        
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new ("identity_id", nameIdentifier.Value),
            new ("auth_method", "jwt_exchange") 
        };
        
        var internalIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        
        identity.RemoveClaim(nameIdentifier);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()));
        identity.AddClaim(new Claim("identity_id", nameIdentifier.Value));

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
