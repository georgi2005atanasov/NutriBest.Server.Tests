using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.ShippingDiscounts
{
    using System.Net;
    using System.Text.Json;
    using System.Net.Http.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.ShippingDiscounts.Models;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Infrastructure.Extensions;
    using static ErrorMessages.ShippingDiscountController;

    [Collection("Shipping Discounts Controller Tests")]
    public class CreateShippingDiscountIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CreateShippingDiscountIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task CreateShippingDiscount_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = "Bulgaria",
                Description = "TEST DISCOUNT",
                DiscountPercentage = "100",
                EndDate = null,
                MinimumPrice = "100"
            };

            // Act
            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(db!.ShippingDiscounts
                .Any(x => x.Description == "TEST DISCOUNT"));
        }

        [Fact]
        public async Task CreateShippingDiscount_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = "Bulgaria",
                Description = "TEST DISCOUNT",
                DiscountPercentage = "100",
                EndDate = null,
                MinimumPrice = "100"
            };

            // Act
            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(db!.ShippingDiscounts
                .Any(x => x.Description == "TEST DISCOUNT"));
        }

        [Theory]
        [InlineData("Bulgaria", "TEST DISCOUNT", "101", "100")]
        [InlineData("Bulgaria", "TEST DISCOUNT", "pesho", "100")]
        public async Task CreateShippingDiscount_ShouldReturnBadRequest_ForInvalidDiscountPercentage(string countryName,
            string description,
            string discountPercentage,
            string minPrice)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = countryName,
                Description = description,
                DiscountPercentage = discountPercentage,
                EndDate = null,
                MinimumPrice = minPrice
            };

            // Act

            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("DiscountPercentage", result.Key);
            Assert.Equal(InvalidDiscountPercentage, result.Message);
            Assert.True(!db!.ShippingDiscounts
                .Any(x => x.Description == "TEST DISCOUNT"));
        }

        [Theory]
        [InlineData("Bulgaria", "TEST DISCOUNT", "100", "pesho")]
        public async Task CreateShippingDiscount_ShouldReturnBadRequest_ForInvalidMinPrice(string countryName,
            string description,
            string discountPercentage,
            string minPrice)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = countryName,
                Description = description,
                DiscountPercentage = discountPercentage,
                EndDate = null,
                MinimumPrice = minPrice
            };

            // Act
            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("MinimumPrice", result.Key);
            Assert.Equal(PricesMustBeNumbers, result.Message);
            Assert.True(!db!.ShippingDiscounts
                .Any(x => x.Description == "TEST DISCOUNT"));
        }

        [Theory]
        [InlineData("Bulgaria",
            "description with bigger length, bigger than 50 and yes it is very big bigger than the expected limit and also big yes yes yes",
            "100",
            "10")]
        [InlineData("Bulgaria",
            "desr",
            "100",
            "10")]
        public async Task CreateShippingDiscount_ShouldReturnBadRequest_ForInvalidDescription(string countryName,
            string description,
            string discountPercentage,
            string minPrice)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = countryName,
                Description = description,
                DiscountPercentage = discountPercentage,
                EndDate = null,
                MinimumPrice = minPrice
            };

            // Act
            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.True(!db!.ShippingDiscounts
                .Any(x => x.Description == "TEST DISCOUNT"));
        }

        [Fact]
        public async Task CreateShippingDiscount_ShouldReturnBadRequest_ForCountryWithDiscount()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = "Bulgaria",
                Description = "TEST DISCOUNT",
                DiscountPercentage = "100",
                EndDate = null,
                MinimumPrice = "100"
            };

            // Act
            await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);
            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(string.Format(CountryAlreadyHasShippingDiscount, "Bulgaria"),
                         result.Message);
        }

        [Fact]
        public async Task CreateShippingDiscount_ShouldReturnBadRequest_ForUnexistingCountry()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = "Bulgaria",
                Description = "TEST DISCOUNT",
                DiscountPercentage = "100",
                EndDate = null,
                MinimumPrice = "100"
            };

            // Act
            await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);
            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(string.Format(CountryAlreadyHasShippingDiscount, "Bulgaria"),
                         result.Message);
        }

        [Fact]
        public async Task CreateShippingDiscount_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = "Bulgaria",
                Description = "TEST DISCOUNT2",
                DiscountPercentage = "100",
                EndDate = null,
                MinimumPrice = "100"
            };

            // Act
            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(!db!.ShippingDiscounts
                .Any(x => x.Description == "TEST DISCOUNT2"));
        }

        [Fact]
        public async Task CreateShippingDiscount_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = "Bulgaria",
                Description = "TEST DISCOUNT3",
                DiscountPercentage = "100",
                EndDate = null,
                MinimumPrice = "100"
            };

            // Act
            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.True(!db!.ShippingDiscounts
                .Any(x => x.Description == "TEST DISCOUNT3"));
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedBgCities();
            db.SeedDeCities();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
