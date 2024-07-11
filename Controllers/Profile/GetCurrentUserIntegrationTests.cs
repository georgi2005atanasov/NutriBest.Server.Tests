using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Profile
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Profile.Models;
    using NutriBest.Server.Infrastructure.Extensions;
    using static ErrorMessages.ProfileController;

    [Collection("Profile Controller Tests")]
    public class GetCurrentUserIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public GetCurrentUserIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetCurrentUser_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync($"/Profile/Mine");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProfileServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProfileServiceModel();

            Assert.Null(result.Name);
            Assert.Equal("admin@example.com", result.Email);
            Assert.Equal("admin", result.UserName);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            // Act
            var response = await client.GetAsync($"/Profile/Mine");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProfileServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProfileServiceModel();

            Assert.Null(result.Name);
            Assert.Equal("employee@example.com", result.Email);
            Assert.Equal("employee", result.UserName);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldBeExecuted_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.GetAsync($"/Profile/Mine");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProfileServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProfileServiceModel();

            Assert.Null(result.Name);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal("user", result.UserName);
        }

        [Fact]
        public async Task GetCurrentUser_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.GetAsync($"/Profile/Mine");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("", data);
        }


        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedFlavours();
            db.SeedBrands();
            db.SeedCategories();
            db.SeedPackages();
            db.SeedBgCities();
            db.SeedDeCities();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
