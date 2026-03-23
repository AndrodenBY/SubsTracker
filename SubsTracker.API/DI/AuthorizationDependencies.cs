using System.Security.Claims;
using Auth0.AuthenticationApi;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SubsTracker.API.Auth0;
using SubsTracker.API.Extension;
using SubsTracker.API.Helpers;
using SubsTracker.API.Options;

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
            .AddScoped<UserGetOrchestrator>()
            .AddScoped<IAuth0Service, Auth0Service>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = "SmartScheme";
                options.DefaultAuthenticateScheme = "SmartScheme";
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "SubsTracker.Session";
                options.Cookie.HttpOnly = true;
                
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    
                options.Cookie.Path = "/";
                options.Cookie.IsEssential = true;
                
                options.Cookie.Domain = null;
                
                options.ExpireTimeSpan = TimeSpan.FromDays(7); 
                options.SlidingExpiration = true;

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer()
            .AddPolicyScheme("SmartScheme", "JWT or Cookie", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    if (context.Request.Cookies.ContainsKey("SubsTracker.Session"))
                    {
                        return CookieAuthenticationDefaults.AuthenticationScheme;
                    }
                    
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            });

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
        
        services.RegisterOptions<CorsOptions>(CorsOptions.SectionName);
        
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var corsOptions = serviceProvider.GetRequiredService<IOptions<CorsOptions>>().Value;
                
                policy.WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition");
            }));
        
        return services;
    }
}
