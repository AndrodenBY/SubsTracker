using System.Security.Claims;
using Auth0.AuthenticationApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SubsTracker.API.Auth0;
using SubsTracker.API.Extension;
using SubsTracker.API.Helpers;
using SubsTracker.BLL.Interfaces;
using SubsTracker.Domain.Exceptions;

namespace SubsTracker.API.DI;

public static class  AuthorizationDependencies
{
    public static IServiceCollection AddAuthorizationDependencies(this IServiceCollection services)
    {
        services.RegisterOptions<Auth0Options>(Auth0Options.SectionName);
        
        services.AddSingleton<AuthenticationApiClient>(serviceProvider => 
        {
            var options = serviceProvider.GetRequiredService<IOptions<Auth0Options>>().Value;
            return new AuthenticationApiClient(new Uri(options.Authority));
        });
    
        services.AddScoped<UserUpdateOrchestrator>()
            .AddScoped<IAuth0Service, Auth0Service>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                
                options.ExpireTimeSpan = TimeSpan.FromDays(7); 
                options.SlidingExpiration = true;
                
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<Auth0Options>>((options, auth0) =>
            {
                options.Authority = auth0.Value.Authority;
                options.Audience = auth0.Value.Audience;
                
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                };
            });
        
        return services;
    }
}
