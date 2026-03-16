using System.Security.Claims;
using SubsTracker.API.Extension;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Middlewares.Session;

public class SessionMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext, IUserService userService)
    {
        var user = httpContext.User;
        
        if (user.Identity?.IsAuthenticated is not true)
        {
            await next(httpContext);
            return;
        }
        
        var internalId = user.FindFirstValue(ClaimTypes.Name);
        if (!string.IsNullOrEmpty(internalId))
        {
            await next(httpContext);
            return;
        }
        
        var identityId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(identityId))
        {
            await next(httpContext);
            return;
        }
        
        var userDto = await userService.GetByIdentityId(identityId, httpContext.RequestAborted);
        if (userDto is null)
        {
            await next(httpContext);
            return;
        }
        
        await httpContext.SessionSignIn(identityId, userDto.Id);
        
        var identity = (ClaimsIdentity)user.Identity;
        identity.AddClaim(new Claim(ClaimTypes.Name, userDto.Id.ToString()));

        await next(httpContext);
    }
}
