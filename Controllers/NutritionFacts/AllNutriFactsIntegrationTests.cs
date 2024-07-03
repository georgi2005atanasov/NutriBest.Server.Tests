namespace NutriBest.Server.Tests.Controllers.NutritionFacts
{
    using System.Net.Http.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.NutritionsFacts.Models;
    using Infrastructure.Extensions;
    using System.Text.Json;

    [Collection("Nutrition Facts Controller Tests")]
    public class AllNutriFactsIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllNutriFactsIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllNutriFactsEndpoint_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "NutriFactsProduct",
                new List<string>
                {
                    "Creatines"
                },
                "100",
                "NutriBest",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            var nutriFactsToSeed = new NutritionFactsServiceModel
            {
                Carbohydrates = "10",
                EnergyValue = "100",
                Fats = "5",
                Proteins = "15",
                Salt = "0.1",
                SaturatedFats = "30",
                Sugars = "0"
            };

            await client.PostAsJsonAsync("/Products/NutriFacts/1", nutriFactsToSeed);

            // Act
            var response = await client.GetAsync("/Products/NutriFacts/1/NutriFactsProduct");
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<NutritionFactsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal("10", result!.Carbohydrates);
            Assert.Equal("100", result.EnergyValue);
            Assert.Equal("5", result.Fats);
            Assert.Equal("15", result.Proteins);
            Assert.Equal("0,1", result.Salt); // SETTINGS
            Assert.Equal("30", result.SaturatedFats);
            Assert.Equal("0", result.Sugars);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedPackages();
            db.SeedCategories();
            db.SeedFlavours();
            db.SeedBrands();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
