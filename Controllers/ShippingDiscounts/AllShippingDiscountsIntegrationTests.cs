namespace NutriBest.Server.Tests.Controllers.ShippingDiscounts
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.ShippingDiscounts.Models;
    using Infrastructure.Extensions;

    [Collection("Shipping Discounts Controller Tests")]
    public class AllShippingDiscountsIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllShippingDiscountsIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllShippingDiscountsEndpoint_ShouldBeExecuted()
        {
            await SeedingHelper.SeedShippingDiscount(clientHelper,
                "Bulgaria",
                "TEST DISCOUNT",
                "100",
                null,
                "100");

            await SeedingHelper.SeedShippingDiscount(clientHelper,
                "Germany",
                "TEST DISCOUNT",
                "100",
                null,
                "100");

            var client = await clientHelper.GetAdministratorClientAsync();

            var response = await client.GetAsync("/ShippingDiscount/All");
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AllShippingDiscountsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllShippingDiscountsServiceModel();

            Assert.Equal(2, result.ShippingDiscounts.Count);
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
