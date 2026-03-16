using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace SubsTracker.API.Extension;

public static class AuthenticationExtension
{
    public static async Task SessionSignIn(
        this HttpContext httpContext, 
        string identityId, 
        Guid internalId, 
        bool isPersistent = true)
    {
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, identityId),
            new (ClaimTypes.Name, internalId.ToString()),
            new ("auth_method", "jwt_exchange") 
        };

        var claimsIdentity = new ClaimsIdentity(
            claims, 
            CookieAuthenticationDefaults.AuthenticationScheme);

        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
