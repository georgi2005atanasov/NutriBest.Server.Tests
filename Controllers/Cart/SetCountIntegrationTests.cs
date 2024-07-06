using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Cart
{
    using System.Web;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Net.Http.Json;
    using System.Net.Http.Headers;
    using Xunit;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Carts.Models;
    using NutriBest.Server.Shared.Responses;
    using Infrastructure.Extensions;
    using static ErrorMessages.CartController;

    [Collection("Cart Controller Tests")]
    public class SetCountIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public SetCountIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task SetCount_ShouldBeExecuted_AndShouldAlsoSetCookie()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var cartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 10,
                Price = 15.99m,
                ProductId = 1
            };

            // Act
            var response = await client.PostAsJsonAsync("/Cart/Set", cartProductModel);
            var cookies = response.Headers.GetValues("Set-Cookie");
            var shoppingCartCookie = cookies.FirstOrDefault(cookie => cookie.StartsWith("ShoppingCart="));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(shoppingCartCookie);
        }

        [Fact]
        public async Task AddToCart_ShouldReturnBadRequest_WhenQuantityIsNegative()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var cartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = -10,
                Price = 15.99m,
                ProductId = 1
            };

            // Act
            var response = await client.PostAsJsonAsync("/Cart/Set", cartProductModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidProductCount, result.Message);
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
            await SeedingHelper.SeedSevenProducts(clientHelper);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
