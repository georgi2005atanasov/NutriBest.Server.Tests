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
    public class DeleteUserIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private UserManager<User>? userManager;

        private IServiceScope? scope;

        public DeleteUserIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnSuccessResponse()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var user = await userManager!.FindByNameAsync("user");


            // Act
            var response = await client.DeleteAsync($"/Admin/DeleteUser/{user.Id}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(db!.Profiles.First(x => x.UserId == user.Id).IsDeleted);
            Assert.False(user.IsDeleted);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnFailResponse_WhenUserDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var fakeUserId = "fakeId";

            // Act
            var response = await client.DeleteAsync($"/Admin/DeleteUser/{fakeUserId}");
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
            var response = await client.DeleteAsync($"/Admin/DeleteUser/{user.Id}?role={roleToAdd}");

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
            var response = await client.DeleteAsync($"/Admin/DeleteUser/{user.Id}?role={roleToAdd}");

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
            var response = await client.DeleteAsync($"/Admin/DeleteUser/{user.Id}?role={roleToAdd}");

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