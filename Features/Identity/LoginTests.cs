namespace NutriBest.Server.Tests.Features.Identity
{
    using Moq;
    using Microsoft.AspNetCore.Mvc;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Identity.Models;
    using NutriBest.Server.Tests.Utilities;
    using NutriBest.Server.Shared.Responses;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;
    using System.Security.Claims;
    using NutriBest.Server.Tests.Fixtures;
    using System.Reflection;

    public class LoginTests : IClassFixture<IdentityTestsFixture>
    {
        private readonly IdentityTestsFixture fixture;

        public LoginTests(IdentityTestsFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public void LoginEndpoint_ShouldHave_HttpPostAndRouteAttributes()
        {
            // Arrange
            var type = typeof(IdentityController);
            var method = type.GetMethod("Login");

            // Act
            Assert.NotNull(method);

            var hasAttributes = AttributeChecker.HasAttributes(method!, typeof(HttpPostAttribute), typeof(RouteAttribute));

            // Assert
            Assert.True(hasAttributes, "The Login endpoint does not have the required HttpPost and Route attributes.");
        }

        [Fact]
        public void LoginEndpoint_ShouldBeLogin()
        {
            // Arrange
            var type = typeof(IdentityController);
            var method = type.GetMethod("Login");

            // Act
            Assert.NotNull(method);

            var routeAttr = method!.GetCustomAttribute<RouteAttribute>();

            // Assert
            Assert.NotNull(routeAttr);
            Assert.Equal("Login", routeAttr!.Template);
        }

        [Fact]
        public async Task Login_ShouldBeValid_WithValidInformation()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique11",
                Email = "uniqueEmail11@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            await fixture.IdentityController.Register(registerModel);

            var result = await fixture.IdentityController.Login(new LoginServiceModel
            {
                UserName = "unique11",
                Password = "Test@1234"
            });

            // Assert
            Assert.NotNull(result);
            ValidateToken(result.Value ?? "");
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WithInvalidPassword()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique12",
                Email = "uniqueEmail12@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            await fixture.IdentityController.Register(registerModel);

            var result = await fixture.IdentityController.Login(new LoginServiceModel
            {
                UserName = "unique13",
                Password = "Test@123"
            });

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WithInvalidUserName()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique14",
                Email = "uniqueEmail14@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            await fixture.IdentityController.Register(registerModel);

            var result = await fixture.IdentityController.Login(new LoginServiceModel
            {
                UserName = "unique119",
                Password = "Test@1234"
            });

            // Assert
            Assert.IsType<UnauthorizedResult>(result.Result);
        }

        [Fact]
        public async Task Notification_ShouldBeSent_AfterSuccessfullLogin()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique15",
                Email = "uniqueEmail15@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            await fixture.IdentityController.Register(registerModel);

            var result = await fixture.IdentityController.Login(new LoginServiceModel
            {
                UserName = "unique15",
                Password = "Test@1234"
            });

            // Assert
            fixture.NotificationServiceMock
                .Verify(n => n.SendNotificationToAdmin("success",
                $"'unique15' Has Just Logged In!"));
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var identityServiceMock = new Mock<IIdentityService>();

            // Set up the UserManager to throw an exception
            identityServiceMock
                .Setup(x => x.FindUserByUserName(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var mockedIdentityController = new IdentityController(identityServiceMock.Object,
                null!,
                null!,
                null!,
                null!);

            var resetModel = new RegisterServiceModel
            {
                Email = "test@example.com",
                Password = "newpassword123",
                ConfirmPassword = "newpassword123",
                UserName = "exceptionUserName2"
            };

            // Act
            await fixture.IdentityController.Register(resetModel);
            var result = await mockedIdentityController.Login(new LoginServiceModel
            {
                UserName = "exceptionUserName2",
                Password = "newpassword123"
            });

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var successResponse = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Something went wrong!", successResponse.Message);
        }

        private void ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("your-secret-key-here"); // Use the same secret key used in ApplicationSettings
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                Assert.NotNull(jwtToken);

                var userNameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;
                Assert.Equal("unique11", userNameClaim);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Assert.NotNull(userIdClaim);
            }
            catch (Exception ex)
            {
                Assert.True(false, $"Token validation failed: {ex.Message}");
            }
        }
    }
}
