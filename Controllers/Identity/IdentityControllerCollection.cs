namespace NutriBest.Server.Tests.Controllers.Identity
{
    using Xunit;

    [CollectionDefinition("Identity Controller Tests")]
    public class IdentityControllerCollection : ICollectionFixture<CustomWebApplicationFactoryFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
