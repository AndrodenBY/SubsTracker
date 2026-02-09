namespace SubsTracker.IntegrationTests.Wiring;

public class ApplicationLayerServiceRegisterTests
{
    [Fact]
    public void RegisterApplicationLayerDependencies_ShouldRegisterKeyServices()
    {
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddOptions();
        services.AddDistributedMemoryCache();
        
        var env = Substitute.For<IWebHostEnvironment>();
        env.EnvironmentName.Returns("Development");
        services.AddSingleton(env);
        
        services.AddSingleton(new DiagnosticListener("Microsoft.AspNetCore"));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Auth0:Authority"] = "https://test.auth0.com",
                ["Auth0:Audience"] = "test-api"
            })
            .Build();

        //Act
        services.RegisterApplicationLayerDependencies(configuration);
        
        var provider = services.BuildServiceProvider(new ServiceProviderOptions 
        { 
            ValidateOnBuild = true 
        });

        //Assert
        provider.GetRequiredService<IMapper>().ShouldNotBeNull();
    }
}
