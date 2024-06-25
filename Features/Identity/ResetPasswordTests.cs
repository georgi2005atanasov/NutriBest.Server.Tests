//namespace NutriBest.Server.Tests.Features.Identity
//{
//    using Moq;
//    using Microsoft.AspNetCore.Identity;
//    using Microsoft.AspNetCore.Mvc;
//    using NutriBest.Server.Data.Models;
//    using NutriBest.Server.Features.Identity;
//    using NutriBest.Server.Features.Identity.Models;
//    using NutriBest.Server.Tests.Utilities;
//    using NutriBest.Server.Shared.Responses;
//    using NutriBest.Server.Tests.Fixtures;
//    using System.Reflection;

//    public class ResetPasswordTests : IClassFixture<IdentityTestsFixture>
//    {
//        private readonly IdentityTestsFixture fixture;

//        public ResetPasswordTests(IdentityTestsFixture fixture) 
//            => this.fixture = fixture;

//        [Fact]
//        public void ResetPasswordEndpoint_ShouldHave_HttpPutAndRouteAttributes()
//        {
//            // Arrange
//            var type = typeof(IdentityController);
//            var method = type.GetMethod("ResetPassword");

//            // Act
//            Assert.NotNull(method);

//            var hasAttributes = AttributeChecker.HasAttributes(method!, typeof(HttpPutAttribute), typeof(RouteAttribute));

//            // Assert
//            Assert.True(hasAttributes, "The ResetPassword endpoint does not have the required HttpPost and Route attributes.");
//        }

//        [Fact]
//        public void ResetPasswordEndpoint_ShouldBeResetPassword()
//        {
//            // Arrange
//            var type = typeof(IdentityController);
//            var method = type.GetMethod("ResetPassword");

//            // Act
//            Assert.NotNull(method);

//            var routeAttr = method!.GetCustomAttribute<RouteAttribute>();

//            // Assert
//            Assert.NotNull(routeAttr);
//            Assert.Equal("ResetPassword", routeAttr!.Template);
//        }

//        [Fact]
//        public async Task ResetPassword_WithValidParameters_ShouldBeValid()
//        {
//            // Arrange
//            var registerModel = new RegisterServiceModel
//            {
//                UserName = "unique16",
//                Email = "uniqueEmail16@example.com",
//                Password = "Test@1234",
//                ConfirmPassword = "Test@1234"
//            };

//            // Act
//            await fixture.IdentityController.Register(registerModel);

//            var token = await fixture.UserManager
//                .GeneratePasswordResetTokenAsync(await fixture.UserManager
//                .FindByEmailAsync("uniqueEmail16@example.com"));

//            // Assert
//            var response = await fixture.IdentityController.ResetPassword(new ResetPasswordServiceModel
//            {
//                NewPassword = "Test@12345",
//                ConfirmPassword = "Test@12345",
//                Email = registerModel.Email,
//                Token = token
//            });

//            var result = Assert.IsType<OkObjectResult>(response);
//            var returnedData = Assert.IsType<SuccessResponse>(result.Value);
//            Assert.Equal("Password reset successful.", returnedData.Message);
//        }

//        [Fact]
//        public async Task ResetPassword_ShouldReturnOkResponse_WithWrongEmail()
//        {
//            // Arrange
//            var registerModel = new RegisterServiceModel
//            {
//                UserName = "unique17",
//                Email = "uniqueEmail17@example.com",
//                Password = "Test@1234",
//                ConfirmPassword = "Test@1234"
//            };

//            // Act
//            await fixture.IdentityController.Register(registerModel);

//            var token = await fixture.UserManager
//                .GeneratePasswordResetTokenAsync(await fixture.UserManager
//                .FindByEmailAsync("uniqueEmail17@example.com"));

//            // Assert
//            var response = await fixture.IdentityController.ResetPassword(new ResetPasswordServiceModel
//            {
//                NewPassword = "Test@12345",
//                ConfirmPassword = "Test@12345",
//                Email = "wrongEmail@example.com",
//                Token = token
//            });

//            var result = Assert.IsType<OkObjectResult>(response);
//            var returnedData = Assert.IsType<SuccessResponse>(result.Value);
//            Assert.Equal("Password reset successful.", returnedData.Message);
//        }

//        [Fact]
//        public async Task ResetPassword_WithInvalidParameters_ShouldReturnBadRequest()
//        {
//            // Arrange
//            var registerModel = new RegisterServiceModel
//            {
//                UserName = "unique18",
//                Email = "uniqueEmail18@example.com",
//                Password = "Test@1234",
//                ConfirmPassword = "Test@1234"
//            };

//            // Act
//            await fixture.IdentityController.Register(registerModel);

//            var token = "fakeToken123";

//            // Assert
//            var response = await fixture.IdentityController.ResetPassword(new ResetPasswordServiceModel
//            {
//                NewPassword = "Test@12345",
//                ConfirmPassword = "Test@12345",
//                Email = registerModel.Email,
//                Token = token
//            });

//            var result = Assert.IsType<BadRequestObjectResult>(response);
//            var returnedData = Assert.IsType<FailResponse>(result.Value);
//            Assert.Equal("Error resetting password.", returnedData.Message);
//        }

//        [Fact]
//        public async Task ResetPassword_ShouldReturnBadRequest_WhenExceptionIsThrown()
//        {
//            // Arrange
//            var mockUserManager = new Mock<UserManager<User>>(
//                Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);

//            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

//            var mockedIdentityController = new IdentityController(null!,
//                mockUserManager.Object,
//                null!,
//                null!,
//                null!);

//            var resetModel = new ResetPasswordServiceModel
//            {
//                Email = "test@example.com",
//                NewPassword = "newpassword123",
//                ConfirmPassword = "newpassword123",
//                Token = "valid-token"
//            };

//            // Act
//            var result = await mockedIdentityController.ResetPassword(resetModel);

//            // Assert
//            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//            var failResponse = Assert.IsType<FailResponse>(badRequestResult.Value);
//            Assert.Equal("Something went wrong!", failResponse.Message);
//        }
//    }
//}
