namespace NutriBest.Server.Tests.Controllers.Orders
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Orders.Models;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Orders Controller Tests")]
    public class OrderRelatedProductsIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public OrderRelatedProductsIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task OrderRelatedProducts_ShouldBeFetched()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Orders/RelatedProducts?price=10");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<OrderRelatedProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new OrderRelatedProductsServiceModel();

            Assert.Equal(4, result.Products.Count);
            Assert.True(result.Products
                .TrueForAll(x => x.Price >= 10));

            foreach (var product in result.Products)
            {
                Assert.NotNull(product.Name);
                Assert.NotNull(product.Flavour);
                Assert.NotNull(product.Quantity);
            }
        }

        [Fact]
        public async Task OrderRelatedProducts_ShouldBeFetched_ButNotAllOfThemAreValid()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Orders/RelatedProducts?price=500.99");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<OrderRelatedProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new OrderRelatedProductsServiceModel();

            Assert.Equal(2, result.Products.Count);
            Assert.True(result.Products
                .TrueForAll(x => x.Price >= 500.99m));

            foreach (var product in result.Products)
            {
                Assert.NotNull(product.Name);
                Assert.NotNull(product.Flavour);
                Assert.NotNull(product.Quantity);
            }
        }

        [Fact]
        public async Task OrderRelatedProducts_ShouldBeFetched_ButNoneOfThemIsValid()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Orders/RelatedProducts?price=5000.99");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<OrderRelatedProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new OrderRelatedProductsServiceModel();

            Assert.Empty(result.Products);
            Assert.True(result.Products
                .TrueForAll(x => x.Price >= 5000.99m));
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
