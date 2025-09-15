using FluentValidation;
using FluentValidation.AspNetCore;
using SubsTracker.API.Mapper;
using SubsTracker.API.Validators.User;
using SubsTracker.BLL;

namespace SubsTracker.API;

public static class ApplicationLayerServiceRegister
{
    public static IServiceCollection RegisterApplicationLayerDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterBusinessLayerDependencies(configuration)
        .AddAutoMapper(cfg => { }, typeof(ViewModelMappingProfile).Assembly)
        .AddFluentValidationAutoValidation()
        .AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
        return services;
    }
}
