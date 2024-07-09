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

    [Collection("Profile Controller Tests")]
    public class GetAddressIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public GetAddressIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetAddress_ShouldBeExecuted_ForAdmin()
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

            await client.PostAsJsonAsync("/Profile/Address", addressModel);

            // Act
            var response = await client.GetAsync("/Profile/Address");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProfileAddressServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProfileAddressServiceModel();

            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("9000", result.StreetNumber);
            Assert.Null(result.PostalCode);
        }

        [Fact]
        public async Task GetAddress_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var addressModel = new ProfileAddressServiceModel
            {
                Country = "Bulgaria",
                City = "Sofia",
                StreetNumber = "12",
                Street = "Karlovska"
            };

            await client.PostAsJsonAsync("/Profile/Address", addressModel);

            // Act
            var response = await client.GetAsync("/Profile/Address");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProfileAddressServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProfileAddressServiceModel();

            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Sofia", result.City);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("12", result.StreetNumber);
            Assert.Null(result.PostalCode);
        }

        [Fact]
        public async Task GetAddress_ShouldBeExecuted_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            // Act
            var response = await client.GetAsync("/Profile/Address");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProfileAddressServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProfileAddressServiceModel();

            Assert.Null(result.Country);
            Assert.Null(result.City);
            Assert.Null(result.Street);
            Assert.Null(result.StreetNumber);
            Assert.Null(result.PostalCode);
        }

        [Fact]
        public async Task GetAddress_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.GetAsync("/Profile/Address");
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
