using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Profile
{
    using System.Net;
    using System.Text.Json;
    using System.Net.Http.Json;
    using Xunit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Profile.Models;
    using Infrastructure.Extensions;
    using static ErrorMessages.ProfileController;

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

        [Fact]
        public async Task SetAddress_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var addressModel = new ProfileAddressServiceModel
            {
                Country = "Bulgaria",
                City = "Sofia",
                StreetNumber = "123",
                Street = "Peshovska"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Profile/Address", addressModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("1", data);

            var address = await db!.Addresses
                .FirstOrDefaultAsync(x => x.StreetNumber == "123");
            
            Assert.NotNull(address);
            Assert.Equal("Bulgaria", address!.Country.CountryName);
            Assert.Equal("Sofia", address.City!.CityName);
            Assert.Equal("Peshovska", address.Street);
        }

        [Fact]
        public async Task SetAddress_ShouldBeExecuted_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var addressModel = new ProfileAddressServiceModel
            {
                Country = "Bulgaria",
                City = "Burgas",
                StreetNumber = "123",
                Street = "Peshovska"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Profile/Address", addressModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("1", data);

            var address = await db!.Addresses
                .FirstOrDefaultAsync(x => x.StreetNumber == "123");

            Assert.NotNull(address);
            Assert.Equal("Bulgaria", address!.Country.CountryName);
            Assert.Equal("Burgas", address.City!.CityName);
            Assert.Equal("Peshovska", address.Street);
        }

        [Fact]
        public async Task SetAddress_ShouldBeExecuted_AndShouldAlsoDeletePreviousAddress()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var addressModel = new ProfileAddressServiceModel
            {
                Country = "Bulgaria",
                City = "Burgas",
                StreetNumber = "123",
                Street = "Peshovska"
            };

            // Act
            await client.PostAsJsonAsync("/Profile/Address", addressModel);

            addressModel.Street = "Karlovska";

            var response = await client.PostAsJsonAsync("/Profile/Address", addressModel);

            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("2", data);

            var address = await db!.Addresses
                .FirstOrDefaultAsync(x => x.StreetNumber == "123");

            Assert.NotNull(address);
            Assert.Equal("Bulgaria", address!.Country.CountryName);
            Assert.Equal("Burgas", address.City!.CityName);
            Assert.Equal("Peshovska", address.Street);
            Assert.Equal(1, db.Addresses
                            .Where(x => x.StreetNumber == "123" && !x.IsDeleted)
                            .Count());
            Assert.Equal(1, db.Addresses
                            .Where(x => x.StreetNumber == "123" && x.IsDeleted)
                            .Count());
        }

        [Fact]
        public async Task SetAddress_ShouldReturnBadRequest_WhenCityIsInvalid()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var addressModel = new ProfileAddressServiceModel
            {
                Country = "Bulgaria",
                City = "Bkokurgas",
                StreetNumber = "123",
                Street = "Peshovska"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Profile/Address", addressModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidCityOrCountry, result.Message);
        }


        [Fact]
        public async Task SetAddress_ShouldReturnBadRequest_WheCountryIsInvalid()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var addressModel = new ProfileAddressServiceModel
            {
                Country = "InvalidCountry",
                City = "Burgas",
                StreetNumber = "123",
                Street = "Peshovska"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Profile/Address", addressModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidCityOrCountry, result.Message);
        }


        [Fact]
        public async Task SetAddress_ShouldReturnBadRequest_WhenCityAndCountryIAreInvalid()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var addressModel = new ProfileAddressServiceModel
            {
                Country = "invalidCountry",
                City = "Bkokurgas",
                StreetNumber = "123",
                Street = "Peshovska"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Profile/Address", addressModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidCityOrCountry, result.Message);
        }

        [Fact]
        public async Task SetAddress_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var addressModel = new ProfileAddressServiceModel
            {
                Country = "Bulgaria",
                City = "Sofia",
                StreetNumber = "123",
                Street = "Peshovska"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Profile/Address", addressModel);
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
