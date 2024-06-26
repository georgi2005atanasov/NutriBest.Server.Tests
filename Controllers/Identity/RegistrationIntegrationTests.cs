namespace NutriBest.Server.Tests.Controllers.Identity
{
    using Moq;
    using Xunit;
    using System.Net;
    using System.Text.Json;
    using System.Net.Http.Json;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Identity.Models;
    using NutriBest.Server.Tests.Controllers.Identity.Data;

    [Collection("Identity Controller Tests")]
    public class RegistrationIntegrationTests
    {
        private readonly CustomWebApplicationFactoryFixture fixture;
        private readonly ClientHelper clientHelper;

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

            var client = clientHelper.GetAnonymousClientAsync();

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
        }

        [Theory]
        [InlineData("pesho2", "pesho2@example.com", "Pesho12345")]
        public async Task RegisterEndpoint_ShouldSendNotification(string userName,
            string email, string password)
        {
            var registerModel = new RegisterServiceModel
            {
                UserName = userName,
                Email = email,
                Password = password,
                ConfirmPassword = password
            };

            var client = clientHelper.GetAnonymousClientAsync();

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

            var client = clientHelper.GetAnonymousClientAsync();

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

            var client = clientHelper.GetAnonymousClientAsync();

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
            var client = clientHelper.GetAnonymousClientAsync();
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

            var client = clientHelper.GetAnonymousClientAsync();

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
    }
}
