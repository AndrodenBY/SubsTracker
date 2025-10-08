namespace SubsTracker.IntegrationTests.Helpers;

[CollectionDefinition("NonParallelTests", DisableParallelization = true)]
public class NonParallelTestsCollection : ICollectionFixture<TestsWebApplicationFactory>;
