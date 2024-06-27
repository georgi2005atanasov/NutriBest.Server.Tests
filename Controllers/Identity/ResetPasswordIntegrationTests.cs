namespace NutriBest.Server.Tests.Controllers.Identity
{
    using Xunit;
    using System.Text.Json;
    using System.Net.Http.Json;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Identity.Models;

    [Collection("Identity Controller Tests")]
    public class ResetPasswordIntegrationTests : IAsyncLifetime
    {
        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private UserManager<User>? userManager;

        private IServiceScope? scope;

        public ResetPasswordIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnSuccess()
        {
            //Arrange
            var registerModel = new RegisterServiceModel
            {
                UserName = "pesho69",
                Email = "pesho69@example.com",
                Password = "Pesho12345",
                ConfirmPassword = "Pesho12345"
            };

            // Act
            var client = clientHelper.GetAnonymousClient();
            await client.PostAsJsonAsync("/Identity/Register", registerModel);

            var user = await userManager!.FindByNameAsync("pesho69");
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var response = await client.PutAsJsonAsync("/Identity/ResetPassword", new ResetPasswordServiceModel
            {
                Email = "pesho69@example.com",
                NewPassword = "Pesho12345",
                ConfirmPassword = "Pesho12345",
                Token = token
            });
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var successResult = JsonSerializer
                .Deserialize<SuccessResponse>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new SuccessResponse();

            Assert.Equal("Password reset successful.", successResult.Message);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnFailResponse_WithDifferentPasswords()
        {
            //Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var user = await userManager!.FindByNameAsync("user");
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            Assert.NotNull(token);
            var response = await client.PutAsJsonAsync("/Identity/ResetPassword", new ResetPasswordServiceModel
            {
                Email = user.Email,
                NewPassword = "Password1234!",
                ConfirmPassword = "Password1239!",
                Token = token
            });
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var successResult = JsonSerializer
                .Deserialize<FailResponse>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new FailResponse();

            Assert.Equal("NewPassword", successResult.Key);
            Assert.Equal("Both passwords should match!", successResult.Message);
        }

        [Fact]
        public async Task ResetPassword_ShouldReturnSuccess_WithUnexistingUser()
        {
            //Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var user = await userManager!.FindByNameAsync("user");
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            Assert.NotNull(token);
            var response = await client.PutAsJsonAsync("/Identity/ResetPassword", new ResetPasswordServiceModel
            {
                Email = user.Email + "invalid",
                NewPassword = "Password1234!",
                ConfirmPassword = "Password1234!",
                Token = token
            });
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var successResult = JsonSerializer
                .Deserialize<SuccessResponse>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new SuccessResponse();

            Assert.Equal("Password reset successful.", successResult.Message);
        }

        // I skipped the case when an Exception is thrown; If all other integration tests
        // are valid, this case becomes impossible

        // this is the reason i can use ! on the userManager and the scope
        // without throwing an exception
        public async Task InitializeAsync()
        {
            await Task.Run(() =>
            {
                scope = fixture.Factory.Services.CreateScope();
                var scopedServices = scope.ServiceProvider;
                userManager = scopedServices.GetRequiredService<UserManager<User>>();
            });
        }

        public Task DisposeAsync()
        {
            scope!.Dispose();
            return Task.CompletedTask;
        }
    }
}
