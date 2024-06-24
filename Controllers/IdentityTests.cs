namespace NutriBest.Server.Tests.Controllers
{
    using Moq;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Identity.Models;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Infrastructure.Services;
    using NutriBest.Server.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Extensions;
    using Microsoft.Extensions.Options;
    using System.IdentityModel.Tokens.Jwt;
    using System.Text;
    using Microsoft.IdentityModel.Tokens;
    using System.Security.Claims;
    using NutriBest.Server.Tests.Utilities;
    using System.Reflection;
    using NutriBest.Server.Shared.Responses;
    using Microsoft.AspNetCore.Authorization;

    public class IdentityTests
    {
        private readonly NutriBestDbContext db;
        private readonly ServiceProvider serviceProvider;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IdentityController identityController;
        private readonly Mock<ICurrentUserService> currentUserServiceMock;
        private readonly Mock<IEmailService> emailServiceMock;
        private readonly Mock<INotificationService> notificationServiceMock;

        public IdentityTests()
        {
            var services = new ServiceCollection();
            services.AddDbContext<NutriBestDbContext>(options =>
                options.UseInMemoryDatabase(new Guid().ToString()));

            services.AddIdentity();

            services.AddLogging();

            notificationServiceMock = new Mock<INotificationService>();
            emailServiceMock = new Mock<IEmailService>();
            currentUserServiceMock = new Mock<ICurrentUserService>();

            var appSettings = new ApplicationSettings
            {
                Secret = "your-secret-key-here"
            };
            services.Configure<ApplicationSettings>(opts => opts.Secret = appSettings.Secret);

            services.AddTransient(_ => notificationServiceMock.Object);
            services.AddTransient(_ => emailServiceMock.Object);
            services.AddTransient(_ => currentUserServiceMock.Object);
            services.AddTransient<IIdentityService, IdentityService>(provider =>
            {
                var userManager = provider.GetRequiredService<UserManager<User>>();
                var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
                var dbContext = provider.GetRequiredService<NutriBestDbContext>();
                var options = provider.GetRequiredService<IOptions<ApplicationSettings>>();
                return new IdentityService(userManager, options, roleManager, dbContext);
            });

            serviceProvider = services.BuildServiceProvider();

            db = serviceProvider.GetRequiredService<NutriBestDbContext>();
            userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            appSettings = new ApplicationSettings
            {
                Secret = "your-secret-key-here"
            };

            services.Configure<ApplicationSettings>(opts => opts.Secret = appSettings.Secret);

            var identityService = serviceProvider.GetRequiredService<IIdentityService>();

            identityController = new IdentityController(
                identityService,
                userManager,
                serviceProvider.GetRequiredService<ICurrentUserService>(),
                emailServiceMock.Object,
                notificationServiceMock.Object
            );

            SeedData();
        }

        private void SeedData()
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Seed roles
            var roles = new[] { "Administrator", "Employee", "User" };
            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    roleManager.CreateAsync(new IdentityRole(role)).Wait();
                }
            }
        }

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
            var result = await identityController.Register(registerModel);

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
            var result = await identityController.Register(registerModel);

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
            var result = await identityController.Register(registerModel);

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
            var result = await identityController.Register(registerModel);
            notificationServiceMock.Verify(n => n.SendNotificationToAdmin("success",
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
            await identityController.Register(existingModel);
            var result = await identityController.Register(registerModel);

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
            await identityController.Register(existingModel);
            var result = await identityController.Register(registerModel);

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
            var result = await identityController.Register(registerModel);

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
            var result = await identityController.Register(registerModel);

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
            var result = await identityController.Register(registerModel);

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
            var result = await identityController.Register(registerModel);

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
            var result = await identityController.Register(registerModel);

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
                Password = "newpassword123",
                ConfirmPassword = "newpassword123",
                UserName = "exceptionUserName1"
            };

            // Act
            var result = await mockedIdentityController.Register(resetModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var successResponse = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Something went wrong!", successResponse.Message);
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
            await identityController.Register(registerModel);

            var result = await identityController.Login(new LoginServiceModel
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
            await identityController.Register(registerModel);

            var result = await identityController.Login(new LoginServiceModel
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
            await identityController.Register(registerModel);

            var result = await identityController.Login(new LoginServiceModel
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
            await identityController.Register(registerModel);

            var result = await identityController.Login(new LoginServiceModel
            {
                UserName = "unique15",
                Password = "Test@1234"
            });

            // Assert
            notificationServiceMock
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
            await identityController.Register(resetModel);
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

        [Fact]
        public void ResetPasswordEndpoint_ShouldHave_HttpPutAndRouteAttributes()
        {
            // Arrange
            var type = typeof(IdentityController);
            var method = type.GetMethod("ResetPassword");

            // Act
            Assert.NotNull(method);

            var hasAttributes = AttributeChecker.HasAttributes(method!, typeof(HttpPutAttribute), typeof(RouteAttribute));

            // Assert
            Assert.True(hasAttributes, "The ResetPassword endpoint does not have the required HttpPost and Route attributes.");
        }

        [Fact]
        public void ResetPasswordEndpoint_ShouldBeResetPassword()
        {
            // Arrange
            var type = typeof(IdentityController);
            var method = type.GetMethod("ResetPassword");

            // Act
            Assert.NotNull(method);

            var routeAttr = method!.GetCustomAttribute<RouteAttribute>();

            // Assert
            Assert.NotNull(routeAttr);
            Assert.Equal("ResetPassword", routeAttr!.Template);
        }

        [Fact]
        public async Task ResetPassword_WithValidParameters_ShouldBeValid()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique16",
                Email = "uniqueEmail16@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            await identityController.Register(registerModel);

            var token = await userManager
                .GeneratePasswordResetTokenAsync(await userManager
                .FindByEmailAsync("uniqueEmail16@example.com"));

            // Assert
            var response = await identityController.ResetPassword(new ResetPasswordServiceModel
            {
                NewPassword = "Test@12345",
                ConfirmPassword = "Test@12345",
                Email = registerModel.Email,
                Token = token
            });

            var result = Assert.IsType<OkObjectResult>(response);
            var returnedData = Assert.IsType<SuccessResponse>(result.Value);
            Assert.Equal("Password reset successful.", returnedData.Message);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnOkResponse_WithWrongEmail()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique17",
                Email = "uniqueEmail17@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            await identityController.Register(registerModel);

            var token = await userManager
                .GeneratePasswordResetTokenAsync(await userManager
                .FindByEmailAsync("uniqueEmail17@example.com"));

            // Assert
            var response = await identityController.ResetPassword(new ResetPasswordServiceModel
            {
                NewPassword = "Test@12345",
                ConfirmPassword = "Test@12345",
                Email = "wrongEmail@example.com",
                Token = token
            });

            var result = Assert.IsType<OkObjectResult>(response);
            var returnedData = Assert.IsType<SuccessResponse>(result.Value);
            Assert.Equal("Password reset successful.", returnedData.Message);
        }

        [Fact]
        public async Task ResetPassword_WithInvalidParameters_ShouldReturnBadRequest()
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "unique18",
                Email = "uniqueEmail18@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            // Act
            await identityController.Register(registerModel);

            var token = "fakeToken123";

            // Assert
            var response = await identityController.ResetPassword(new ResetPasswordServiceModel
            {
                NewPassword = "Test@12345",
                ConfirmPassword = "Test@12345",
                Email = registerModel.Email,
                Token = token
            });

            var result = Assert.IsType<BadRequestObjectResult>(response);
            var returnedData = Assert.IsType<FailResponse>(result.Value);
            Assert.Equal("Error resetting password.", returnedData.Message);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var mockUserManager = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null!, null!, null!, null!, null!, null!, null!, null!);

            mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ThrowsAsync(new Exception());

            var mockedIdentityController = new IdentityController(null!,
                mockUserManager.Object,
                null!,
                null!,
                null!);

            var resetModel = new ResetPasswordServiceModel
            {
                Email = "test@example.com",
                NewPassword = "newpassword123",
                ConfirmPassword = "newpassword123",
                Token = "valid-token"
            };

            // Act
            var result = await mockedIdentityController.ResetPassword(resetModel);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var failResponse = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Something went wrong!", failResponse.Message);
        }

        [Fact]
        public void RolesEndpoint_ShouldHave_HttpGetAndAuthorizeAndRouteAttributes()
        {
            // Arrange
            var type = typeof(IdentityController);
            var method = type.GetMethod("Roles");

            // Act
            Assert.NotNull(method);
            var hasAttributes = AttributeChecker.HasAttributes(method!, typeof(HttpGetAttribute), typeof(RouteAttribute), typeof(AuthorizeAttribute));

            // Assert
            Assert.True(hasAttributes, "The Login endpoint does not have the required HttpPost, Authorize and Route attributes.");
        }

        [Fact]
        public void RolesEndpoint_ShouldBeRoles()
        {
            // Arrange
            var type = typeof(IdentityController);
            var method = type.GetMethod("Roles");

            // Act
            Assert.NotNull(method);

            var routeAttr = method!.GetCustomAttribute<RouteAttribute>();

            // Assert
            Assert.NotNull(routeAttr);
            Assert.Equal("Roles", routeAttr!.Template);
        }

        [Fact]
        public void RolesEndpoint_ShouldHaveAuthorizeAttributeWithRoleOfAdministrator()
        {
            // Arrange
            var type = typeof(IdentityController);
            var method = type.GetMethod("Roles");

            // Act
            Assert.NotNull(method);

            var authorizeAttr = method!.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(authorizeAttr);
            Assert.Equal("Administrator", authorizeAttr!.Roles);
        }

        [Fact]
        public async void RolesEndpoint_ShouldReturnAllRolesWithoutAdministrator()
        {
            // Act
            var result = await identityController.Roles();

            // Assert
            Assert.NotNull(result);
            var successResult = Assert.IsType<OkObjectResult>(result.Result);
            var rolesModel = Assert.IsType<RolesServiceModel>(successResult.Value);
            Assert.Contains("Employee", rolesModel.Roles);
            Assert.Contains("User", rolesModel.Roles);
            Assert.DoesNotContain("Administrator", rolesModel.Roles);
        }

        [Fact]
        public async Task RolesEndpoint_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var mockIdentityService = new Mock<IIdentityService>();

            mockIdentityService
                .Setup(x => x.AllRoles())
                .Throws(new Exception());

            var mockedIdentityController = new IdentityController(mockIdentityService.Object,
                null!,
                null!,
                null!,
                null!);

            // Act
            var result = await mockedIdentityController.Roles();

            // Assert
            var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var failResponse = Assert.IsType<FailResponse>(badRequestResult.Value);
            Assert.Equal("Error occured when fetching the roles!", failResponse.Message);
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

                // Validate claims
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