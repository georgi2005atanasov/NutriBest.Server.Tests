namespace NutriBest.Server.Tests.Controllers.Identity
{
    using Xunit;
    using Moq;
    using System.Net;
    using System.Text.Json;
    using System.Net.Http.Json;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Identity.Models;
    using NutriBest.Server.Tests.Controllers.Identity.Data;
    using NutriBest.Server.Data;
    using Microsoft.AspNetCore.Identity;
    using NutriBest.Server.Data.Models;
    using Microsoft.EntityFrameworkCore;

    [Collection("Identity Controller Tests")]
    public class RegistrationIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private CustomWebApplicationFactoryFixture fixture;

        private UserManager<User>? userManager;

        private ClientHelper clientHelper;

        private IServiceScope? scope;

        public RegistrationIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            this.fixture = fixture;
            clientHelper = new ClientHelper(fixture);
        }

        [Theory]
        [InlineData("pesho", "pesho@example.com", "Pesho12345")]
        public async Task RegisterEndpoint_ShouldReturnOkResult(string userName,
            string email, string password)
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = userName,
                Email = email,
                Password = password,
                ConfirmPassword = password
            };

            fixture.Factory.NotificationServiceMock!
                .Setup(x => x.SendNotificationToAdmin(It.IsAny<string>(),
                                                      It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.PostAsJsonAsync("/Identity/Register", registerModel);
            var data = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            var result = JsonSerializer.Deserialize<SuccessResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new SuccessResponse();

            // Assert
            Assert.Equal("Successfully added new user!", result.Message);
            Assert.Contains(userName, db!.Users.Select(x => x.UserName));
            Assert.NotNull(db!.Profiles
                            .Where(x => x.UserId == db
                                                .Users
                                                .First(x => x.UserName == userName).Id));
        }

        [Theory]
        [InlineData("pesho2", "pesho2@example.com", "Pesho12345")]
        public async Task RegisterEndpoint_ShouldSendNotification(string userName,
            string email, string password)
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = userName,
                Email = email,
                Password = password,
                ConfirmPassword = password
            };

            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.PostAsJsonAsync("/Identity/Register", registerModel);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SuccessResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new SuccessResponse();

            // Assert
            fixture.Factory.NotificationServiceMock!.Verify(n => n.SendNotificationToAdmin("success",
            $"'pesho2' Has Just Registered!"), Times.Once);
        }

        [Theory]
        [InlineData("pesho3", "pesho3@example.com", "Pesho12345", "Pesho123456")]
        public async Task RegisterEndpoint_ShouldReturnBadRequest_WhenPasswordsAreDifferent(string userName,
            string email, string password, string confirmPassword)
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = userName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.PostAsJsonAsync("/Identity/Register", registerModel);

            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal("Password", result.Key);
            Assert.Equal("Both passwords should match!", result.Message);
            Assert.DoesNotContain(userName, db!.Users.Select(x => x.UserName));
        }

        [Theory]
        [InlineData("pe sho4", "pesho4@example.com", "Pesho12345", "Pesho12345")]
        public async Task RegisterEndpoint_ShouldReturnBadRequest_WhenUsernameHasWhiteSpaces(string userName,
            string email, string password, string confirmPassword)
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = userName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.PostAsJsonAsync("/Identity/Register", registerModel);

            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal("UserName", result.Key);
            Assert.Equal("Username must not contain white spaces!", result.Message);
            Assert.DoesNotContain(userName, db!.Users.Select(x => x.UserName));
        }

        [Theory]
        [MemberData(nameof(InvalidIdentityData.Data), MemberType = typeof(InvalidIdentityData))]
        public async Task RegisterEndpoint_ShouldReturnBadRequestWithErrors_WhenUserCannotBeCreated(string userName,
            string email, string password, string confirmPassword)
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = userName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            // Act
            var client = clientHelper.GetAnonymousClient();
            var response = await client.PostAsJsonAsync("/Identity/Register", registerModel);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("pesho5", "pesho5@example.com", "Pesho12345")]
        public async Task RegisterEndpoint_ShouldReturnBadRequest_WhenExceptionIsThrown(string userName,
            string email, string password)
        {
            // Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = userName,
                Email = email,
                Password = password,
                ConfirmPassword = password
            };

            fixture.Factory.NotificationServiceMock!
                .Setup(x => x.SendNotificationToAdmin(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<Exception>();

            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.PostAsJsonAsync("/Identity/Register", registerModel);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal("Something went wrong!", result.Message);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();

            await Task.Run(() =>
            {
                scope = fixture.Factory.Services.CreateScope();
                var scopedServices = scope.ServiceProvider;
                db = scopedServices.GetRequiredService<NutriBestDbContext>();
                userManager = scopedServices.GetRequiredService<UserManager<User>>();
            });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
