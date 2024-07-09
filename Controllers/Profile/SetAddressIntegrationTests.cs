namespace NutriBest.Server.Tests.Controllers.Profile
{
    using System.Net;
    using System.Text.Json;
    using System.Net.Http.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Profile.Models;
    using Infrastructure.Extensions;
    using Microsoft.EntityFrameworkCore;

    [Collection("Profile Controller Tests")]
    public class SetAddressIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public SetAddressIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task SetAddress_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var addressModel = new ProfileAddressServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                StreetNumber = "9000",
                Street = "Karlovska"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Profile/Address", addressModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("1", data);

            var address = await db!.Addresses
                .FirstOrDefaultAsync(x => x.StreetNumber == "9000");

            Assert.NotNull(address);
            Assert.Equal("Bulgaria", address!.Country.CountryName);
            Assert.Equal("Plovdiv", address.City!.CityName);
            Assert.Equal("Karlovska", address.Street);
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
