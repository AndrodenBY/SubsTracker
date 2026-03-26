using System.Net;
using System.Security.Claims;
using Auth0.AuthenticationApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens; 
using SubsTracker.API.Auth.IdentityProvider;
using SubsTracker.API.Auth.Session;
using SubsTracker.API.Constants;
using SubsTracker.API.Extension;
using SubsTracker.API.Helpers;
using SubsTracker.API.Options;
using CookieOptions = SubsTracker.API.Options.CookieOptions;

namespace SubsTracker.API.DI;

public static class  AuthorizationDependencies
{
    public static IServiceCollection AddAuthorizationDependencies(this IServiceCollection services)
    {
        services.AddCorsConfiguration()
            .AddAuth0Infrastructure()
            .AddAuthenticationScheme();

        return services;
    }
        
    private static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.RegisterOptions<CorsOptions>(CorsOptions.SectionName)
            .ValidateOnStart()
            .ValidateDataAnnotations();

        services.AddCors();
        services.AddOptions<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>()
            .PostConfigure<IOptions<CorsOptions>>((options, cors) =>
            {
                var corsOptions = cors.Value;
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithExposedHeaders(corsOptions.ContentDisposition);
                });
            });

        return services;
    }
        
    private static IServiceCollection AddAuth0Infrastructure(this IServiceCollection services)
    {
        services.RegisterOptions<Auth0Options>(Auth0Options.SectionName)
            .ValidateOnStart()
            .ValidateDataAnnotations();

        services.AddSingleton<AuthenticationApiClient>(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<Auth0Options>>().Value;
            return new AuthenticationApiClient(new Uri(options.Authority));
        });

        services.AddScoped<UserUpdateOrchestrator>()
            .AddScoped<UserGetOrchestrator>()
            .AddScoped<IClaimsTransformation, ClaimsTransformer>()
            .AddScoped<IAuth0Service, Auth0Service>();

        return services;
    } 
        
    private static IServiceCollection AddAuthenticationScheme(this IServiceCollection services)
    {
        services.RegisterOptions<CookieOptions>(CookieOptions.SectionName)
            .ValidateOnStart()
            .ValidateDataAnnotations();

        services.AddAuthentication(options => 
        {
            options.DefaultScheme = AuthSchemeConstants.SchemeName;
            options.DefaultAuthenticateScheme = AuthSchemeConstants.SchemeName;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddJwtBearer()
            .AddPolicyScheme(AuthSchemeConstants.SchemeName, AuthSchemeConstants.SchemeDescription , options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var cookieOptions = context.RequestServices.GetRequiredService<IOptions<CookieOptions>>().Value;
                    if (context.Request.Cookies.ContainsKey(cookieOptions.Name))
                    {
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    }
                    
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    
                    return authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        ? JwtBearerDefaults.AuthenticationScheme
                        : CookieAuthenticationDefaults.AuthenticationScheme;
                };
            });
            
        services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<IOptions<CookieOptions>>((options, cookie) => 
            {
                var cookieOptions = cookie.Value;
                    
                options.Cookie.Name = cookieOptions.Name;
                options.Cookie.HttpOnly = cookieOptions.HttpOnly;
                options.Cookie.SameSite = cookieOptions.SameSite;
                options.Cookie.SecurePolicy = cookieOptions.SecurePolicy;
                options.Cookie.Path = cookieOptions.Path;
                options.ExpireTimeSpan = TimeSpan.FromDays(cookieOptions.ExpirationTimeSpan);
                options.SlidingExpiration = cookieOptions.SlidingExpiration;
                    
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = redirectContext =>
                    {
                        redirectContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized; 
                        return Task.CompletedTask;
                    }
                };
            });
            
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<Auth0Options>>((options, auth0) =>
                {
                    var auth0Options = auth0.Value;
                    
                    options.Authority = auth0Options.Authority;
                    options.Audience = auth0Options.Audience;
                    
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
