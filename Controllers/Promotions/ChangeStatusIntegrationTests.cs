using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Promotions
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Infrastructure.Extensions;
    using static ErrorMessages.PromotionsController;

    [Collection("Promotions Controller Tests")]
    public class ChangeStatusIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public ChangeStatusIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task ChangePromotionStatus_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "Klean Athlete",
                "TEST PROMO",
                null,
                DateTime.Now,
                "40");

            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
            Assert.Single(db!.Promotions);

            // Act
            var response = await client.PutAsync("/Promotions/Status/1", null);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == true));
            Assert.Equal("true", data);
        }

        [Fact]
        public async Task ChangePromotionStatus_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "Klean Athlete",
                "TEST PROMO",
                null,
                DateTime.Now,
                "40");

            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
            Assert.Single(db!.Promotions);

            // Act
            var response = await client.PutAsync("/Promotions/Status/1", null);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == true));
            Assert.Equal("true", data);
        }

        [Fact]
        public async Task ChangePromotionStatus_ShouldReturnBadRequest_BecauseOfStartDate()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "Klean Athlete",
                "TEST PROMO",
                null,
                DateTime.Now.AddDays(10),
                "40");

            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
            Assert.Single(db!.Promotions);

            // Act
            var response = await client.PutAsync("/Promotions/Status/1", null);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
            Assert.Equal(CannotChangePromotionStatus, result.Message);
        }

        [Fact]
        public async Task ChangePromotionStatus_ShouldReturnBadRequest_BecauseItDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "Klean Athlete",
                "TEST PROMO",
                null,
                DateTime.Now.AddDays(10),
                "40");

            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
            Assert.Single(db!.Promotions);

            // Act
            var response = await client.PutAsync("/Promotions/Status/2", null);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
            Assert.Equal(InvalidPromotion, result.Message);
        }

        [Fact]
        public async Task ChangePromotionStatus_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "Klean Athlete",
                "TEST PROMO",
                null,
                DateTime.Now,
                "40");

            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
            Assert.Single(db!.Promotions);

            // Act
            var response = await client.PutAsync("/Promotions/Status/1", null);

            // Assert
            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ChangePromotionStatus_ShouldReturnUnauthorized_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "Klean Athlete",
                "TEST PROMO",
                null,
                DateTime.Now,
                "40");

            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
            Assert.Single(db!.Promotions);

            // Act
            var response = await client.PutAsync("/Promotions/Status/1", null);

            // Assert
            Assert.NotNull(db!.Promotions
                .FirstOrDefault(x => x.IsActive == false));
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
