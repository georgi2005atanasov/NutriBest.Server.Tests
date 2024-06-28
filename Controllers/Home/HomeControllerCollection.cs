namespace NutriBest.Server.Tests.Controllers.Home
{
    using Xunit;

    [CollectionDefinition("Home Controller Tests")]
    public class HomeControllerCollection : ICollectionFixture<CustomWebApplicationFactoryFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
