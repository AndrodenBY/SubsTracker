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
        
        var identity = (ClaimsIdentity)user.Identity!;
        var nameIdentifier = identity.FindFirst(ClaimTypes.NameIdentifier);
        if (nameIdentifier is null || Guid.TryParse(nameIdentifier.Value, out _))
        {
            await next(httpContext);
            return;
        }
        
        var userDto = await userService.GetByIdentityId(nameIdentifier.Value, httpContext.RequestAborted);
        if (userDto is not null)
        {
            await httpContext.SessionLogin(nameIdentifier.Value, userDto.Id);
        
            identity.RemoveClaim(nameIdentifier);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userDto.Id.ToString()));
            identity.AddClaim(new Claim("identity_id", nameIdentifier.Value));
        }
        
        await next(httpContext);
    }
}
