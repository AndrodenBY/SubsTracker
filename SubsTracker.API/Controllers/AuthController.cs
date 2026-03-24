using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubsTracker.API.Auth.Session;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IUserService userService) : ControllerBase
{
    [Authorize]
    [HttpPost("login")]
    public async Task Login(CancellationToken cancellationToken)
    {
        await HttpContext.SessionLogin(userService, cancellationToken); 
    }

    [HttpPost("logout")]
    public async Task Logout()
    {
        await HttpContext.SessionLogout();
    }
}
