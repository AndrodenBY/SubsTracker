using Microsoft.Extensions.Options;

namespace SubsTracker.API.Extension;

public static class OptionsRegistration
{
    public static OptionsBuilder<TOptions> RegisterOptions<TOptions>(this IServiceCollection services, string sectionName) 
        where TOptions : class
    {
        return services.AddOptions<TOptions>()
            .BindConfiguration(sectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
