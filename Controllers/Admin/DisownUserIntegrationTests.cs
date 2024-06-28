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
    public class DisownUserIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private UserManager<User>? userManager;

        private IServiceScope? scope;

        public DisownUserIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task DisownUser_ShouldReturnSuccessResponse()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var user = await userManager!.FindByNameAsync("user");
            var roleToRemove = "User";

            // Act
            var response = await client.PatchAsync($"/Admin/Disown/{user.Id}?role={roleToRemove}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<SuccessResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new SuccessResponse();

            // Assert
            Assert.Equal($"Successfully removed role '{roleToRemove}' from '{user.UserName}'!", 
                string.Format(result.Message, roleToRemove, user.UserName));
            var userRoles = await userManager.GetRolesAsync(user);
            Assert.False(userRoles.Contains(roleToRemove));
        }

        [Fact]
        public async Task DisownUser_ShouldReturnFailResponse_WithUnexistingUserPassed()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var roleToAdd = "User";

            // Act
            var response = await client.PatchAsync($"/Admin/Disown/invalidId?role={roleToAdd}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal("User could not be found!", result.Message);
        }

        [Fact]
        public async Task DisownUser_ShouldReturnFailResponse_WithInvalidRolePassed()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var user = await userManager!.FindByNameAsync("user");
            var roleToRemove = "InvalidRole";

            // Act
            var response = await client.PatchAsync($"/Admin/Disown/{user.Id}?role={roleToRemove}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal("Invalid role!", result.Message);
            var userRoles = await userManager.GetRolesAsync(user);
            Assert.False(userRoles.Contains(roleToRemove));
        }

        [Fact]
        public async Task DisownUser_ShouldReturnFailResponse_ForUserWithoutTheRole()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var user = await userManager!.FindByNameAsync("user");
            var roleToRemove = "Employee";

            // Act
            var response = await client.PatchAsync($"/Admin/Disown/{user.Id}?role={roleToRemove}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal($"The user does not have this role!", result.Message);
            var userRoles = await userManager.GetRolesAsync(user);
            Assert.False(userRoles.Contains(roleToRemove));
        }

        [Fact]
        public async Task DisownUser_ShouldReturnUnauthorized_ForEmployees()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var user = await userManager!.FindByNameAsync("employee");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PatchAsync($"/Admin/Disown/{user.Id}?role={roleToAdd}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DisownUser_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            var user = await userManager!.FindByNameAsync("user");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PatchAsync($"/Admin/Disown/{user.Id}?role={roleToAdd}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task DisownUser_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var user = await userManager!.FindByNameAsync("user");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PatchAsync($"/Admin/Disown/{user.Id}?role={roleToAdd}", null);

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
