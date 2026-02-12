using System.Security.Claims;
using Auth0.AuthenticationApi;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SubsTracker.API.Auth0;
using SubsTracker.API.Helpers;
using SubsTracker.API.Mapper;
using SubsTracker.API.Validators.User;
using SubsTracker.BLL;
using SubsTracker.Domain.Options;

namespace SubsTracker.API;

public static class ApplicationLayerServiceRegister
{
    public static void RegisterApplicationLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterBusinessLayerDependencies(configuration)
            .AddAutoMapper(_ => { }, typeof(ViewModelMappingProfile).Assembly)
            .AddControllers();
        services.AddFluentValidationAutoValidation()
            .AddFluentValidationClientsideAdapters()
            .AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();

        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
            ));
        
        services.AddOptions<Auth0Options>()
            .BindConfiguration(Auth0Options.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart(); 
        
        services.AddSingleton<IAuthenticationApiClient>(serviceProvider => 
        {
            var options = serviceProvider.GetRequiredService<IOptions<Auth0Options>>().Value;
            return new AuthenticationApiClient(new Uri(options.Authority));
        });
    
        services.AddScoped<UserUpdateOrchestrator>()
            .AddScoped<IAuth0Service, Auth0Service>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
    }
}
