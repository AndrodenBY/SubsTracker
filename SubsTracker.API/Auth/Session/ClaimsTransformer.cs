using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using SubsTracker.API.Constants;
using SubsTracker.BLL.Interfaces;

namespace SubsTracker.API.Auth.Session;

public class ClaimsTransformer(IUserService userService) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var nameIdentifier = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(nameIdentifier, out _))
        {
            return principal;
        }
        
        var identityId = principal.FindFirstValue(ClaimsConstants.Sub) ?? nameIdentifier;
        if (string.IsNullOrEmpty(identityId))
        {
            return principal;
        }

        
        var userDto = await userService.GetByIdentityId(identityId, CancellationToken.None);
        if (userDto is null)
        {
            return principal;
        }
        
        var claims = new List<Claim>
        {
            new (ClaimTypes.NameIdentifier, userDto.Id.ToString()),
            new ("identity_id", identityId),
            new ("auth_method", "transformed_jwt")
        };

        var identity = new ClaimsIdentity(claims, principal.Identity?.AuthenticationType);
        return new ClaimsPrincipal(identity);
    }
}
