namespace NutriBest.Server.Tests.Features.Admin
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using NutriBest.Server.Features;
    using NutriBest.Server.Features.Admin;
    using NutriBest.Server.Features.Admin.Models;
    using NutriBest.Server.Features.Identity.Models;
    using NutriBest.Server.Tests.Fixtures;
    using NutriBest.Server.Tests.Utilities;
    using System.Reflection;

    public class AdminControllerTests : IClassFixture<AdminTestsFixture>
    {
        private readonly AdminTestsFixture fixture;

        public AdminControllerTests(AdminTestsFixture fixture) 
            => this.fixture = fixture;

        [Fact]
        public void AdminController_ShouldHaveAuthorizeAttributeWithValueOfAdministrator()
        {
            // Arrange
            var controllerType = typeof(AdminController);
            var authorizeAttr = controllerType.GetCustomAttribute(typeof(AuthorizeAttribute)) as AuthorizeAttribute;

            // Act
            Assert.NotNull(authorizeAttr);
            var roles = authorizeAttr!.Roles;

            // Assert
            Assert.Equal("Administrator", roles);
        }

        [Fact]
        public void AdminController_ShouldInheritApiController()
        {
            // Arrange
            var controllerType = typeof(AdminController);

            // Assert
            Assert.Equal(typeof(ApiController), controllerType.BaseType);
        }

        [Fact]
        public void AllUsersEndpoint_ShouldHave_HttpGetAndRouteAttributes()
        {
            // Arrange
            var type = typeof(AdminController);
            var method = type.GetMethod("AllUsers");

            // Act
            Assert.NotNull(method);

            var hasAttributes = AttributeChecker.HasAttributes(method!, typeof(HttpGetAttribute), typeof(RouteAttribute));

            // Assert
            Assert.True(hasAttributes, "The Login endpoint does not have the required HttpPost and Route attributes.");
        }

        [Fact]
        public async Task AllUsersEndpoint_ShouldReturn_ActionResult_IEnumerableUserServiceModel()
        {
            // Arrange
            await fixture.IdentityController
                .Register(new RegisterServiceModel
                {
                    Email = "unique41@example.com",
                    Password = "12345Pesho",
                    ConfirmPassword = "12345Pesho",
                    UserName = "unique41"
                });

            // Act
            var users = await fixture.AdminController.AllUsers();

            // Assert
            Assert.IsType<ActionResult<IEnumerable<UserServiceModel>>>(users);
        }
    }
}
