namespace NutriBest.Server.Tests.Controllers.Promotions
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Infrastructure.Extensions;
    using NutriBest.Server.Features.Products.Models;

    [Collection("Promotions Controller Tests")]
    public class GetProductsOfPromotionIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public GetProductsOfPromotionIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetProductsOfPromotion_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();

            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            // Act
            var response = await client.GetAsync("/Promotions/1/Products");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<ProductServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ProductServiceModel>();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetProductsOfPromotion_ShouldReturnEmptyList_WhenPromotionDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Promotions/1/Products");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<ProductServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ProductServiceModel>();

            Assert.Empty(result);
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
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
