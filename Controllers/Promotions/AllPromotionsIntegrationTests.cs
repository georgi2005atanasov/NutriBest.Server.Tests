namespace NutriBest.Server.Tests.Controllers.Promotions
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Promotions.Models;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Promotions Controller Tests")]
    public class AllPromotionsIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllPromotionsIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllPromotionsEndpoint_ShouldBeExecuted()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "Klean Athlete",
                "TEST PROMO",
                null,
                DateTime.Now,
                "30");

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "Klean Athlete",
                "TEST PROMO2",
                null,
                DateTime.Now,
                "40");

            // Act
            var response = await client.GetAsync("/Promotions");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<PromotionServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PromotionServiceModel>();

            Assert.Equal(2, result.Count);
            Assert.True(db!.Promotions.Any(x => x.Description == "TEST PROMO"));
            Assert.True(db!.Promotions.Any(x => x.Description == "TEST PROMO2"));
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
