namespace NutriBest.Server.Tests.Controllers.Admin
{
    using Xunit;
    using System.Net;
    using System.Text.Json;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Shared.Responses;

    [Collection("Admin Controller Tests")]
    public class RestoreUserIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private UserManager<User>? userManager;

        private IServiceScope? scope;

        public RestoreUserIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task RestoreUser_ShouldReturnSuccessResponse()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var user = await userManager!.FindByNameAsync("user");

            await client.DeleteAsync($"/Admin/DeleteUser/{user.Id}");
            Assert.True(db!.Profiles.First(x => x.UserId == user.Id).IsDeleted);

            // Act
            var response = await client.PostAsync($"/Admin/Restore/{user.Id}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<SuccessResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new SuccessResponse();

            // Assert
            Assert.Equal($"Successfully restored profile with email '{user.Email}'!", result.Message);
            Assert.False(user.IsDeleted);
        }

        [Fact]
        public async Task RestoreUser_ShouldReturnFailResponse_WhenUserDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var fakeUserId = "fakeId";

            // Act
            var response = await client.PostAsync($"/Admin/Restore/{fakeUserId}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal("Invalid user!", result.Message);
        }

        [Fact]
        public async Task RestoreUser_ShouldReturnUnauthorized_ForEmployees()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var user = await userManager!.FindByNameAsync("employee");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PostAsync($"/Admin/Restore/{user.Id}?role={roleToAdd}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RestoreUser_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            var user = await userManager!.FindByNameAsync("user");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PostAsync($"/Admin/Restore/{user.Id}?role={roleToAdd}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RestoreUser_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var user = await userManager!.FindByNameAsync("user");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PostAsync($"/Admin/Restore/{user.Id}?role={roleToAdd}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();

            await Task.Run(() =>
            {
                scope = fixture.Factory.Services.CreateScope();
                var scopedServices = scope.ServiceProvider;
                userManager = scopedServices.GetRequiredService<UserManager<User>>();
                db = scopedServices.GetRequiredService<NutriBestDbContext>();
            });
        }

        public Task DisposeAsync()
        {
            scope!.Dispose();
            return Task.CompletedTask;
        }
    }
}
