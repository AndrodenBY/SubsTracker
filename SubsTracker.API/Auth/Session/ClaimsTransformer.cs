using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Auth.Session;

public class ClaimsTransformer(IUserService userService) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal claimsPrincipal)
    {
        var nameIdentifier = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(nameIdentifier, out _))
        {
            return claimsPrincipal;
        }
        
        var identityId = claimsPrincipal.FindFirstValue("sub") ?? nameIdentifier;
        if (string.IsNullOrEmpty(identityId))
        {
            return claimsPrincipal;
        }

        
        var userDto = await userService.GetByIdentityId(identityId, CancellationToken.None);
        if (userDto is null)
        {
            return claimsPrincipal;
        }
        
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new ("identity_id", identityId),
            new ("auth_method", "transformed_jwt")
        };

        var identity = new ClaimsIdentity(claims, claimsPrincipal.Identity?.AuthenticationType);
        return new ClaimsPrincipal(identity);
    }
}
