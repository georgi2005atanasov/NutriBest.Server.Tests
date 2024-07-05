namespace NutriBest.Server.Tests.Controllers.PromoCodes
{
    using System.Text.Json;
    using System.Net.Http.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.PromoCodes.Models;
    using Infrastructure.Extensions;
    using System.Net;

    [Collection("Promo Codes Controller Tests")]
    public class CreatePromoCodesIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CreatePromoCodesIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task CreatePromoCode_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var promoCodeModel = new PromoCodeServiceModel
            {
                Count = "100",
                Description = "TEST CODE",
                DiscountPercentage = "10"
            };

            // Act
            var response = await client.PostAsJsonAsync("/PromoCode", promoCodeModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(data);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(result);
        }

        [Fact]
        public async Task CreatePromoCode_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var promoCodeModel = new PromoCodeServiceModel
            {
                Count = "100",
                Description = "TEST CODE",
                DiscountPercentage = "10"
            };

            Assert.Empty(db!.PromoCodes);

            // Act
            var response = await client.PostAsJsonAsync("/PromoCode", promoCodeModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(data);
            Assert.NotEmpty(db!.PromoCodes);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(result);
        }

        [Fact]
        public async Task CreatePromoCode_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var promoCodeModel = new PromoCodeServiceModel
            {
                Count = "100",
                Description = "TEST CODE",
                DiscountPercentage = "10"
            };

            Assert.Empty(db!.PromoCodes);

            // Act
            var response = await client.PostAsJsonAsync("/PromoCode", promoCodeModel);

            // Assert
            Assert.Empty(db!.PromoCodes);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreatePromoCode_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var promoCodeModel = new PromoCodeServiceModel
            {
                Count = "100",
                Description = "TEST CODE",
                DiscountPercentage = "10"
            };

            Assert.Empty(db!.PromoCodes);

            // Act
            var response = await client.PostAsJsonAsync("/PromoCode", promoCodeModel);

            // Assert
            Assert.Empty(db!.PromoCodes);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
