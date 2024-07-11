namespace NutriBest.Server.Tests.Infrastructure
{
    using Xunit;

    [CollectionDefinition("Infrastructure Tests")]
    public class InfrastructureCollection : ICollectionFixture<CustomWebApplicationFactoryFixture>
    {
    }
}
