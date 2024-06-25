namespace NutriBest.Server.Tests.Features.Identity
{
    using NutriBest.Server.Features;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Tests.Fixtures;

    public class IdentityControllerTests  : IClassFixture<IdentityTestsFixture>
    {
        private readonly IdentityTestsFixture fixture;

        public IdentityControllerTests(IdentityTestsFixture fixture) 
            => this.fixture = fixture;

        [Fact]
        public void IdentityController_ShouldInherit_ApiController()
        {
            // Arrange
            var controllerType = typeof(IdentityController);

            // Assert
            Assert.Equal(typeof(ApiController), controllerType.BaseType);
        }
    }
}
