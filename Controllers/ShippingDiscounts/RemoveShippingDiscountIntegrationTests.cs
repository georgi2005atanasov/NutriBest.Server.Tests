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
    using NutriBest.Server.Tests.Extensions;
    using Infrastructure.Extensions;
    using static ErrorMessages.ShippingDiscountController;

    [Collection("Shipping Discounts Controller Tests")]
    public class RemoveShippingDiscountIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public RemoveShippingDiscountIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task RemoveShippingDiscount_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedShippingDiscount(clientHelper,
                "Bulgaria",
                "TEST DISCOUNT",
                "100",
                null,
                "100");

            Assert.NotEmpty(db!.ShippingDiscounts);

            var shippingDiscountModel = new DeleteShippingDiscountServiceModel()
            {
                CountryName = "Bulgaria"
            };

            // Act
            var response = await client.DeleteAsync("ShippingDiscount", shippingDiscountModel);
            var data = await response.Content.ReadAsStringAsync();

            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;

            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(succeeded);
            Assert.True(!db!.ShippingDiscounts
                .Any(x => x.Description == "TEST DISCOUNT"));
        }

        [Fact]
        public async Task RemoveShippingDiscount_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedShippingDiscount(clientHelper,
                "Bulgaria",
                "TEST DISCOUNT",
                "100",
                null,
                "100");

            var shippingDiscountModel = new DeleteShippingDiscountServiceModel()
            {
                CountryName = "Bulgaria"
            };

            Assert.NotEmpty(db!.ShippingDiscounts);

            // Act
            var response = await client.DeleteAsync("ShippingDiscount", shippingDiscountModel);
            var data = await response.Content.ReadAsStringAsync();

            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;

            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(succeeded);
            Assert.True(!db!.ShippingDiscounts
                .Any(x => x.Description == "TEST DISCOUNT"));
        }

        [Fact]
        public async Task RemoveShippingDiscount_ShouldBeExecuted_AndShouldAlsoReturnFalse()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var shippingDiscountModel = new DeleteShippingDiscountServiceModel()
            {
                CountryName = "Bulgaria"
            };

            // Act
            var response = await client.DeleteAsync("ShippingDiscount", shippingDiscountModel);
            var data = await response.Content.ReadAsStringAsync();

            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;

            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(succeeded);
        }

        [Fact]
        public async Task RemoveShippingDiscount_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedShippingDiscount(clientHelper,
                "Bulgaria",
                "TEST DISCOUNT",
                "100",
                null,
                "100");

            // Act
            var response = await client.DeleteAsync("ShippingDiscount");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(db!.ShippingDiscounts
               .Any(x => x.Description == "TEST DISCOUNT"));
        }

        [Fact]
        public async Task RemoveShippingDiscount_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedShippingDiscount(clientHelper,
                "Bulgaria",
                "TEST DISCOUNT",
                "100",
                null,
                "100");

            // Act
            var response = await client.DeleteAsync("ShippingDiscount");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.True(db!.ShippingDiscounts
               .Any(x => x.Description == "TEST DISCOUNT"));
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
