namespace NutriBest.Server.Tests.Controllers.Profile
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Carts.Models;
    using NutriBest.Server.Features.Orders.Models;
    using NutriBest.Server.Features.Profile.Models;
    using NutriBest.Server.Features.UsersOrders.Models;
    using NutriBest.Server.Features.GuestsOrders.Models;
    using NutriBest.Server.Tests.Controllers.Orders.Data;
    using NutriBest.Server.Tests.Controllers.Profile.Data;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Profile Controller Tests")]
    public class DeleteProfileIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public DeleteProfileIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task DeleteUser_ShouldBeExecuted_ForTheCurrentUser()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            Assert.False(db!.Profiles.Where(x => x.IsDeleted).Any());
            Assert.False(db!.Users.Where(x => x.IsDeleted).Any());

            // Act
            var response = await client.DeleteAsync("/Profile");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("true", data);
            Assert.True(db!.Profiles.Where(x => x.IsDeleted).Any());
            Assert.True(db!.Users.Where(x => x.IsDeleted).Any());
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            Assert.False(db!.Profiles.Where(x => x.IsDeleted).Any());
            Assert.False(db!.Users.Where(x => x.IsDeleted).Any());

            // Act
            var response = await client.DeleteAsync("/Profile");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(db!.Profiles.Where(x => x.IsDeleted).Any());
            Assert.False(db!.Users.Where(x => x.IsDeleted).Any());
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnForbidden_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            Assert.False(db!.Profiles.Where(x => x.IsDeleted).Any());
            Assert.False(db!.Users.Where(x => x.IsDeleted).Any());

            // Act
            var response = await client.DeleteAsync("/Profile");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.False(db!.Profiles.Where(x => x.IsDeleted).Any());
            Assert.False(db!.Users.Where(x => x.IsDeleted).Any());
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
