namespace NutriBest.Server.Tests.Controllers.Flavours
{
    using Xunit;
    using System.Text.Json;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Brands.Models;
    using Infrastructure.Extensions;
    using NutriBest.Server.Features.Flavours.Models;

    [Collection("Brands Controller Tests")]
    public class ProductsByFlavourIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public ProductsByFlavourIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task ProductsByBrand_ShouldBeExecuted()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            await SeedingHelper.SeedProduct(clientHelper,
                "product10",
                            new List<string>
                {
                    "Creatines"
                },
                "100",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");
            await SeedingHelper.SeedProduct(clientHelper,
                "product11",
                            new List<string>
                {
                    "Creatines"
                },
                "100",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");
            await SeedingHelper.SeedProduct(clientHelper,
                "product12",
                            new List<string>
                {
                    "Creatines"
                },
                "100",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            // Act
            var response = await client.GetAsync("/Products/ByFlavourCount");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<FlavourCountServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new List<FlavourCountServiceModel>();

            Assert.Equal(3, result
                            .First(x => x.Name == "Coconut") // Ensure It exists!!!
                            .Count);
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
