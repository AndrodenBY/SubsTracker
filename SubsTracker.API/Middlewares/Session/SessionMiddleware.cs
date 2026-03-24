using System.Security.Claims;
using SubsTracker.API.Auth;
using SubsTracker.API.Auth.Session;
using SubsTracker.API.Extension;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Middlewares.Session;

public class SessionMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext, IUserService userService)
    {
        await httpContext.SessionLogin(userService, httpContext.RequestAborted);
        
        await next(httpContext);
    }
}
 
