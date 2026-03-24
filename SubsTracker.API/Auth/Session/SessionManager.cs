using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Auth.Session;

public static class SessionManager
{
    public static async Task SessionLogin(this HttpContext httpContext, IUserService userService, CancellationToken cancellationToken)
    {
        await IdentityManager.ValidateIdentity(httpContext, cancellationToken);
        var internalPrincipal = await IdentityManager.TransformRequestClaims(httpContext, userService, cancellationToken);
        
        if (internalPrincipal is not null)
        {
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                IssuedUtc = DateTimeOffset.UtcNow,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                internalPrincipal,
                authProperties);
        }
    }
    
    public static async Task SessionLogout(this HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
