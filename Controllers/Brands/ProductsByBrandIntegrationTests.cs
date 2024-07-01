using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Brands
{
    using Xunit;
    using System.Net;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Brands.Models;
    using Infrastructure.Extensions;
    using NutriBest.Server.Features.Products.Models;
    using NutriBest.Server.Features.Promotions.Models;
    using static ErrorMessages.BrandsController;
    using System.Text.Json;
    using NutriBest.Server.Shared.Responses;

    [Collection("Brands Controller Tests")]
    public class ProductsByBrandIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public ProductsByBrandIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task ProductsByBrand_ShouldBeExecuted()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            await SeedingHelper.SeedProduct(clientHelper, "product10", "Klean Athlete");
            await SeedingHelper.SeedProduct(clientHelper, "product11", "Klean Athlete");
            await SeedingHelper.SeedProduct(clientHelper, "product12", "Klean Athlete");

            // Act
            var response = await client.GetAsync("/Products/ByBrandCount");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<BrandCountServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new List<BrandCountServiceModel>();

            Assert.Equal(3, result
                            .First(x => x.Name == "Klean Athlete")
                            .Count);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedBrands();
            db.SeedCategories();
            db.SeedPackages();
            db.SeedFlavours();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
