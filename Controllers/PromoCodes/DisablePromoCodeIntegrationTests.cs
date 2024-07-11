namespace NutriBest.Server.Tests.Controllers.PromoCodes
{
    using System.Net;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Tests.Extensions;
    using NutriBest.Server.Features.PromoCodes.Models;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Promo Codes Controller Tests")]
    public class DisablePromoCodeIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public DisablePromoCodeIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task DisablePromoCode_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            await SeedingHelper.SeedPromoCode(clientHelper,
                "TEST PROMO CODE 20% OFF",
                "100",
                "20");

            Assert.NotEmpty(db!.PromoCodes);

            // Act
            var response = await client.DeleteAsync("/PromoCode?description=TEST PROMO CODE 20% OFF");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.PromoCodes);
        }

        [Fact]
        public async Task DisablePromoCode_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedPromoCode(clientHelper,
                "TEST PROMO CODE 20% OFF",
                "100",
                "20");

            Assert.NotEmpty(db!.PromoCodes);

            // Act
            var response = await client.DeleteAsync("/PromoCode?description=TEST PROMO CODE 20% OFF");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.PromoCodes);
        }

        [Fact]
        public async Task DisablePromoCode_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            await SeedingHelper.SeedPromoCode(clientHelper,
                "TEST PROMO CODE 20% OFF",
                "100",
                "20");

            Assert.NotEmpty(db!.PromoCodes);

            // Act
            var response = await client.DeleteAsync("/PromoCode?description=TEST PROMO CODE 20% OFF");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEmpty(db!.PromoCodes);
        }

        [Fact]
        public async Task DisablePromoCode_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            await SeedingHelper.SeedPromoCode(clientHelper,
                "TEST PROMO CODE 20% OFF",
                "100",
                "20");

            Assert.NotEmpty(db!.PromoCodes);

            // Act
            var response = await client.DeleteAsync("/PromoCode?description=TEST PROMO CODE 20% OFF");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotEmpty(db!.PromoCodes);
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
