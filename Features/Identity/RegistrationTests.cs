namespace NutriBest.Server.Tests.Features.Identity
{
    using Moq;
    using Microsoft.AspNetCore.Mvc;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Identity.Models;
    using NutriBest.Server.Tests.Utilities;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Tests.Fixtures;
    using System.Reflection;

    public class RegistrationTests : IClassFixture<IdentityTestsFixture>
    {
        private readonly IdentityTestsFixture fixture;

        public RegistrationTests(IdentityTestsFixture fixture) 
            => this.fixture = fixture;

        [Fact]
        public void Register_EndpointShouldHave_HttpPostAndRouteAttributes()
        {
            // Arrange
            var type = typeof(IdentityController);
            var method = type.GetMethod("Register");

            // Act
            Assert.NotNull(method);

            var hasAttributes = AttributeChecker.HasAttributes(method!, typeof(HttpPostAttribute), typeof(RouteAttribute));

            // Assert
            Assert.True(hasAttributes, "The Register endpoint does not have the required HttpPost and Route attributes.");
        }

        [Fact]
        public void RegisterEndpointShouldBeRegister()
        {
            // Arrange
            var type = typeof(IdentityController);
            var method = type.GetMethod("Register");

            // Act
            Assert.NotNull(method);

            var routeAttr = method!.GetCustomAttribute<RouteAttribute>();

            // Assert
            Assert.NotNull(routeAttr);
            Assert.Equal("Register", routeAttr!.Template);
        }

        [Fact]
        public async Task Register_WithValidModel_ShouldReturnOk()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique1",
                Email = "uniqueEmail1@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var resultValue = Assert.IsType<SuccessResponse>(okResult.Value);
            Assert.Equal("Successfully added new user!", resultValue.Message);
        }

        [Fact]
        public async Task Register_WithDifferentPasswords_ShouldReturn_FailResponse()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique2",
                Email = "uniqueEmail2@example.com",
                Password = "Test@1235",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
            var resultValue = Assert.IsType<FailResponse>(badResult.Value);
            Assert.Equal("Password", resultValue.Key);
            Assert.Equal("Both passwords should match!", resultValue.Message);
        }

        [Fact]
        public async Task Register_WithInvalidUserName_ShouldReturn_FailResponse()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "test user",
                Email = "uniqueEmail3@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badResult.StatusCode);
            var resultValue = Assert.IsType<FailResponse>(badResult.Value);
            Assert.Equal("UserName", resultValue.Key);
            Assert.Equal("Username must not contain white spaces!", resultValue.Message);
        }

        [Fact]
        public async Task Notification_ShouldBeSent_AfterSuccessfullRegister()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique3",
                Email = "uniqueEmail4@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            var result = await fixture.IdentityController.Register(registerModel);
            fixture.NotificationServiceMock.Verify(n => n.SendNotificationToAdmin("success",
                $"'{registerModel.UserName}' Has Just Registered!"));
        }

        [Fact]
        public async Task Errors_ShouldBeReturned_AfterRegisteringUserWithAlreadyExistingUserName()
        {
            // Arrange
            var existingModel = new RegisterServiceModel
            {
                UserName = "sameUserName",
                Email = "uniqueEmail5@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            var registerModel = new RegisterServiceModel
            {
                UserName = "sameUserName",
                Email = "uniqueEmail6@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            await fixture.IdentityController.Register(existingModel);
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultValue = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal($"Username '{registerModel.UserName}' is already taken.", resultValue.Message);
        }

        [Fact]
        public async Task Errors_ShouldBeReturned_AfterRegisteringUserWithAlreadyExistingEmail()
        {
            // Arrange
            var existingModel = new RegisterServiceModel
            {
                UserName = "unique4",
                Email = "existingEmail@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            var registerModel = new RegisterServiceModel
            {
                UserName = "unique5",
                Email = "existingEmail@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            await fixture.IdentityController.Register(existingModel);
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultValue = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal($"Email '{registerModel.Email}' is already taken.", resultValue.Message);
        }

        [Fact]
        public async Task Error_ShouldBeReturned_AfterRegisteringWithShortPassword()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique6",
                Email = "uniqueEmail7@example.com",
                Password = "111Abv",
                ConfirmPassword = "111Abv"
            };

            // Act
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultValue = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Passwords must be at least 9 characters.", resultValue.Message);
        }

        [Fact]
        public async Task Errors_ShouldBeReturned_AfterRegistering_WithPasswordWithoutCapitalLetters()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique7",
                Email = "uniqueEmail8@example.com",
                Password = "111abv111",
                ConfirmPassword = "111abv111"
            };

            // Act
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultValue = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Passwords must have at least one uppercase ('A'-'Z').", resultValue.Message);
        }

        [Fact]
        public async Task Errors_ShouldBeReturned_AfterRegistering_WithPasswordWithoutSmallLetters()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique8",
                Email = "uniqueEmail9@example.com",
                Password = "111ABV111",
                ConfirmPassword = "111ABV111"
            };

            // Act
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultValue = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Passwords must have at least one lowercase ('a'-'z').", resultValue.Message);
        }

        [Fact]
        public async Task Errors_ShouldBeReturned_AfterRegistering_WithPasswordWithOnlyDigits()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique9",
                Email = "uniqueEmail10@example.com",
                Password = "123456789",
                ConfirmPassword = "123456789"
            };

            // Act
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultValue = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Passwords must have at least one lowercase ('a'-'z')., Passwords must have at least one uppercase ('A'-'Z').", resultValue.Message);
        }

        [Fact]
        public async Task Errors_ShouldBeReturned_AfterRegistering_WithInvalidEmailFormat()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique10",
                Email = "uniqueEmailexample.com",
                Password = "123456789Aa",
                ConfirmPassword = "123456789Aa"
            };

            // Act
            var result = await fixture.IdentityController.Register(registerModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var resultValue = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Email 'uniqueEmailexample.com' is invalid.", resultValue.Message);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var identityServiceMock = new Mock<IIdentityService>();

            // Set up the UserManager to throw an exception
            identityServiceMock
                .Setup(x => x.CreateUser(It.IsAny<string>(),
                                        It.IsAny<string>(),
                                        It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var mockedIdentityController = new IdentityController(identityServiceMock.Object,
                null!,
                null!,
                null!,
                null!);

            var resetModel = new RegisterServiceModel
            {
                Email = "test@example.com",
                Password = "newpassword1234",
                ConfirmPassword = "newpassword1234",
                UserName = "exceptionUserName1"
            };

            // Act
            var result = await mockedIdentityController.Register(resetModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var successResponse = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Something went wrong!", successResponse.Message);
        }
    }
}
