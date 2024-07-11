namespace NutriBest.Server.Tests.Controllers.PromoCodes
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.PromoCodes.Models;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Promo Codes Controller Tests")]
    public class AllPromoCodesIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllPromoCodesIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllPromoCodedEndpoint_ShouldBeExecuted()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedPromoCode(clientHelper,
                "TEST CODE",
                "10",
                "20");

            await SeedingHelper.SeedPromoCode(clientHelper,
                "TEST CODE2",
                "11",
                "21");

            // Act
            var response = await client.GetAsync("/PromoCode");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<PromoCodeByDescriptionServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PromoCodeByDescriptionServiceModel>();

            Assert.Equal(21, db!.PromoCodes.Count());
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
