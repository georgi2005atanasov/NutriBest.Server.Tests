//namespace NutriBest.Server.Tests.Features.Identity
//{
//    using Moq;
//    using Microsoft.AspNetCore.Mvc;
//    using NutriBest.Server.Features.Identity;
//    using NutriBest.Server.Features.Identity.Models;
//    using NutriBest.Server.Tests.Utilities;
//    using NutriBest.Server.Shared.Responses;
//    using Microsoft.AspNetCore.Authorization;
//    using NutriBest.Server.Tests.Fixtures;
//    using System.Reflection;

//    public class RolesTests : IClassFixture<IdentityTestsFixture>
//    {
//        private readonly IdentityTestsFixture fixture;

//        public RolesTests(IdentityTestsFixture fixture) 
//            => this.fixture = fixture;

//        [Fact]
//        public void RolesEndpoint_ShouldHave_HttpGetAndAuthorizeAndRouteAttributes()
//        {
//            // Arrange
//            var type = typeof(IdentityController);
//            var method = type.GetMethod("Roles");

//            // Act
//            Assert.NotNull(method);
//            var hasAttributes = AttributeChecker.HasAttributes(method!, typeof(HttpGetAttribute), typeof(RouteAttribute), typeof(AuthorizeAttribute));

//            // Assert
//            Assert.True(hasAttributes, "The Login endpoint does not have the required HttpPost, Authorize and Route attributes.");
//        }

//        [Fact]
//        public void RolesEndpoint_ShouldBeRoles()
//        {
//            // Arrange
//            var type = typeof(IdentityController);
//            var method = type.GetMethod("Roles");

//            // Act
//            Assert.NotNull(method);

//            var routeAttr = method!.GetCustomAttribute<RouteAttribute>();

//            // Assert
//            Assert.NotNull(routeAttr);
//            Assert.Equal("Roles", routeAttr!.Template);
//        }

//        [Fact]
//        public void RolesEndpoint_ShouldHaveAuthorizeAttributeWithRoleOfAdministrator()
//        {
//            // Arrange
//            var type = typeof(IdentityController);
//            var method = type.GetMethod("Roles");

//            // Act
//            Assert.NotNull(method);

//            var authorizeAttr = method!.GetCustomAttribute<AuthorizeAttribute>();

//            // Assert
//            Assert.NotNull(authorizeAttr);
//            Assert.Equal("Administrator", authorizeAttr!.Roles);
//        }

//        [Fact]
//        public async Task RolesEndpoint_ShouldReturnAllRolesWithoutAdministrator()
//        {
//            // Act
//            var result = await fixture.IdentityController.Roles();

//            // Assert
//            Assert.NotNull(result);
//            var successResult = Assert.IsType<OkObjectResult>(result.Result);
//            var rolesModel = Assert.IsType<RolesServiceModel>(successResult.Value);
//            Assert.Contains("Employee", rolesModel.Roles);
//            Assert.Contains("User", rolesModel.Roles);
//            Assert.DoesNotContain("Administrator", rolesModel.Roles);
//        }

//        [Fact]
//        public async Task RolesEndpoint_ShouldReturnBadRequest_WhenExceptionIsThrown()
//        {
//            // Arrange
//            var mockIdentityService = new Mock<IIdentityService>();

//            mockIdentityService
//                .Setup(x => x.AllRoles())
//                .Throws(new Exception());

//            var mockedIdentityController = new IdentityController(mockIdentityService.Object,
//                null!,
//                null!,
//                null!,
//                null!);

//            // Act
//            var result = await mockedIdentityController.Roles();

//            // Assert
//            var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
//            var failResponse = Assert.IsType<FailResponse>(badRequestResult.Value);
//            Assert.Equal("Error occured when fetching the roles!", failResponse.Message);
//        }
//    }
//}
