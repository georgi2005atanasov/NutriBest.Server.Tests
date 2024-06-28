namespace NutriBest.Server.Tests.Controllers.Identity
{
    using Xunit;
    using Moq;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http.Json;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Identity.Models;

    [Collection("Identity Controller Tests")]
    public class LoginIntegrationTesting : IAsyncLifetime
    {
        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private ApplicationSettings appSettings;

        public LoginIntegrationTesting(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
            using (var scope = fixture.Factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                var options = scopedServices.GetRequiredService<IOptions<ApplicationSettings>>();
                appSettings = options.Value;
            }
        }

        [Fact]
        public async Task LoginEndpoint_ShouldReturnValidToken()
        {
            // Arrange
            var loginModel = new LoginServiceModel
            {
                UserName = "user",
                Password = "Password123!"
            };

            fixture.Factory.NotificationServiceMock!
                .Setup(x => x.SendNotificationToAdmin(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            // Act
            var client = clientHelper.GetAnonymousClient();
            var response = await client.PostAsJsonAsync("/Identity/Login", loginModel);

            // Assert
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadAsStringAsync();
            ValidateToken(token);
        }

        [Theory]
        [InlineData("user", "Password1234!")]
        [InlineData("gosho", "Password123!")]
        public async Task LoginEndpoint_ShouldReturnUnauthorized_WhenCredentialsAreNotValid(string userName,
            string password)
        {
            // Arrange
            var loginModel = new LoginServiceModel
            {
                UserName = userName,
                Password = password
            };

            // Act
            var client = clientHelper.GetAnonymousClient();
            var response = await client.PostAsJsonAsync("/Identity/Login", loginModel);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("gosho29", "gosho29@example.com", "Pesho12345", "Pesho12345")]
        public async Task LoginEndpoint_ShouldReturnUnauthorized_WhenUserIsDeleted(string userName,
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

            var loginModel = new LoginServiceModel
            {
                UserName = userName,
                Password = password
            };

            // Act
            var client = clientHelper.GetAnonymousClient();
            var res = await client.PostAsJsonAsync("/Identity/Register", registerModel);

            client = await clientHelper.GetAuthenticatedClientAsync(userName, password);
            await client.DeleteAsync("/Profile");
            var response = await client.PostAsJsonAsync("/Identity/Login", loginModel);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("user", "Password123!")]
        public async Task LoginEndpoint_ShouldReturnBadRequest_WhenExceptionIsThrown(string userName,
            string password)
        {
            // Arrange
            var loginModel = new LoginServiceModel
            {
                UserName = userName,
                Password = password
            };

            fixture.Factory.NotificationServiceMock!
                .Setup(x => x.SendNotificationToAdmin("success", $"'{userName}' Has Just Logged In!"))
                .Throws<Exception>();

            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.PostAsJsonAsync("/Identity/Login", loginModel);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal("Something went wrong!", result.Message);
        }

        private void ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

            Assert.NotNull(principal);
            Assert.IsType<JwtSecurityToken>(validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;
            var claimToCheck = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
            Assert.Equal("User", claimToCheck != null ? claimToCheck.Value : "");
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
