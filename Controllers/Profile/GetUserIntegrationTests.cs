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
    using Infrastructure.Extensions;
    using static ErrorMessages.ProfileController;

    [Collection("Profile Controller Tests")]
    public class GetUserIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public GetUserIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetUserById_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var user = await clientHelper.GetOtherUserClientAsync();
            var admin = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedUserOrder(clientHelper,
                true,
                "user@example.com",
                "user",
                "TEST USER!!!"); 

            var userToGet = await db!.Users
                .FirstAsync(x => x.UserName == "user");

            // Act
            var response = await admin.GetAsync($"/Profile/{userToGet.Id}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProfileDetailsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProfileDetailsServiceModel();

            Assert.Equal("TEST USER!!!", result.Name);
            Assert.Equal("user", result.UserName);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("0884138832", result.PhoneNumber);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("900", result.StreetNumber);
            Assert.Equal("User", result.Roles);
            Assert.Equal(1, result.TotalOrders);
            Assert.Equal(484.90m, result.TotalSpent);
        }


        [Fact]
        public async Task GetUserById_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var user = await clientHelper.GetOtherUserClientAsync();
            var admin = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedUserOrder(clientHelper,
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            var userToGet = await db!.Users
                .FirstAsync(x => x.UserName == "user");

            // Act
            var response = await admin.GetAsync($"/Profile/{userToGet.Id}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProfileDetailsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProfileDetailsServiceModel();

            Assert.Equal("TEST USER!!!", result.Name);
            Assert.Equal("user", result.UserName);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("0884138832", result.PhoneNumber);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("900", result.StreetNumber);
            Assert.Equal("User", result.Roles);
            Assert.Equal(1, result.TotalOrders);
            Assert.Equal(484.90m, result.TotalSpent);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnBadRequest_SinceUserIdIsInvalid()
        {
            // Arrange
            var admin = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedUserOrder(clientHelper,
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            // Act
            var response = await admin.GetAsync($"/Profile/invalidId");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(UserCouldNotBeFound, result.Message);
        }


        [Fact]
        public async Task GetUserById_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var user = await clientHelper.GetOtherUserClientAsync();
            var anonymous = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedUserOrder(clientHelper,
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            var userToGet = await db!.Users
                .FirstAsync(x => x.UserName == "user");

            // Act
            var response = await anonymous.GetAsync($"/Profile/{userToGet.Id}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("", data);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var user = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedUser(clientHelper,
                "Pesho",
                "pesho@abv.bg",
                "Pesho12345",
                "Pesho12345");

            var anotherUser = await clientHelper
                .GetAuthenticatedClientAsync("Pesho", "Pesho12345");

            await SeedingHelper.SeedUserOrder(clientHelper,
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            var userToGet = await db!.Users
                .FirstAsync(x => x.UserName == "user");

            // Act
            var response = await anotherUser.GetAsync($"/Profile/{userToGet.Id}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
