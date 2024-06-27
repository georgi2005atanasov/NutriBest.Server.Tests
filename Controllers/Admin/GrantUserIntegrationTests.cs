namespace NutriBest.Server.Tests.Controllers.Admin
{
    using Xunit;
    using System.Net;
    using System.Text.Json;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Shared.Responses;

    [Collection("Admin Controller Tests")]
    public class GrantUserIntegrationTests : IAsyncLifetime
    {
        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private UserManager<User>? userManager;

        private IServiceScope? scope;

        public GrantUserIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GrantUser_ShouldReturnSuccessResponse()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var user = await userManager!.FindByNameAsync("user");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PatchAsync($"/Admin/Grant/{user.Id}?role={roleToAdd}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<SuccessResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new SuccessResponse();

            // Assert
            Assert.Equal($"Successfully added role '{roleToAdd}' to 'user'!", result.Message);
        }

        [Fact]
        public async Task GrantUser_ShouldReturnFailResponse_WithUnexistingUserPassed()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var roleToAdd = "Employee";

            // Act
            var response = await client.PatchAsync($"/Admin/Grant/invalidId?role={roleToAdd}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal("User could not be found!", result.Message);
        }

        [Fact]
        public async Task GrantUser_ShouldReturnFailResponse_WithInvalidRolePassed()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var user = await userManager!.FindByNameAsync("user");
            var roleToAdd = "InvalidRole";

            // Act
            var response = await client.PatchAsync($"/Admin/Grant/{user.Id}?role={roleToAdd}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal("Invalid role!", result.Message);
        }

        [Fact]
        public async Task GrantUser_ShouldReturnFailResponse_ForUserInTheSameRole()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var user = await userManager!.FindByNameAsync("user");
            var roleToAdd = "User";

            // Act
            var response = await client.PatchAsync($"/Admin/Grant/{user.Id}?role={roleToAdd}", null);
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            // Assert
            Assert.Equal($"'{user.UserName}' is already in the role of '{roleToAdd}'!", result.Message);
        }

        [Fact]
        public async Task GrantUser_ShouldReturnUnauthorized_ForEmployees()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var user = await userManager!.FindByNameAsync("employee");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PatchAsync($"/Admin/Grant/{user.Id}?role={roleToAdd}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GrantUser_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            var user = await userManager!.FindByNameAsync("user");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PatchAsync($"/Admin/Grant/{user.Id}?role={roleToAdd}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GrantUser_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var user = await userManager!.FindByNameAsync("user");
            var roleToAdd = "Employee";

            // Act
            var response = await client.PatchAsync($"/Admin/Grant/{user.Id}?role={roleToAdd}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

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
