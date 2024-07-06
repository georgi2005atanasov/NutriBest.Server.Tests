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
    public class RemoveFromCartIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public RemoveFromCartIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task RemoveFromCart_ShouldBeExecuted()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var firstCartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 1,
                Price = 15.99m,
                ProductId = 1
            };

            var secondCartProductModel = new CartProductServiceModel
            {
                Flavour = "Lemon Lime",
                Grams = 500,
                Count = 9,
                Price = 50.99m,
                ProductId = 3
            };

            var thirdCartProductModel = new CartProductServiceModel
            {
                Flavour = "Chocolate",
                Grams = 500,
                Count = 3,
                Price = 500.99m,
                ProductId = 6
            };

            // Act
            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Second product addition
            var secondResponse = await client.PostAsJsonAsync("/Cart/Add", secondCartProductModel);
            var updatedCookieHeaderAfterSecond = secondResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (updatedCookieHeaderAfterSecond != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", updatedCookieHeaderAfterSecond);
            }

            // Third product addition
            var thirdResponse = await client.PostAsJsonAsync("/Cart/Add", thirdCartProductModel);
            var updatedCookieHeaderAfterThird = thirdResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (updatedCookieHeaderAfterThird != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", updatedCookieHeaderAfterThird);
            }

            secondCartProductModel.Count = 1;
            var removeResponse = await client.PostAsJsonAsync("/Cart/Remove", secondCartProductModel);

            string cartCookieValue = string.Empty;
            var updatedCookieHeaderAfterRemoving = removeResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (updatedCookieHeaderAfterRemoving != null)
            {
                cartCookieValue = updatedCookieHeaderAfterRemoving.Split(';').FirstOrDefault()!.Split('=').Last();
            }
            var decodedCartCookieValue = HttpUtility.UrlDecode(cartCookieValue);

            //Parse the cart cookie value into CartServiceModel
            var cartServiceModel = JsonSerializer.Deserialize<CartServiceModel>(decodedCartCookieValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new CartServiceModel();

            // Assert
            Assert.Equal(1926.88m, cartServiceModel.TotalProducts);
            Assert.Equal(1926.88m, cartServiceModel.OriginalPrice);
            Assert.Equal(8, cartServiceModel
                            .CartProducts
                            .Where(x => x.Flavour == "Lemon Lime")
                            .First()
                            .Count);
        }

        [Fact]
        public async Task RemoveFromCart_ShouldReturnBadRequest_SinceProductDoesNotExists()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var firstCartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 1,
                Price = 15.99m,
                ProductId = 1
            };

            // Act
            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            firstCartProductModel.Flavour = "FakeFlavour";

            var removeResponse = await client.PostAsJsonAsync("/Cart/Remove", firstCartProductModel);
            var data = await removeResponse.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.NotFound, removeResponse.StatusCode);
            Assert.Equal(ProductNotFound, result.Message);
        }

        [Fact]
        public async Task RemoveFromCart_ShouldBeExecutedProperly_WhenBiggerCountIsBeingRemoved()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var firstCartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 1,
                Price = 15.99m,
                ProductId = 1
            };

            // Act
            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            firstCartProductModel.Count = 2;

            var removeResponse = await client.PostAsJsonAsync("/Cart/Remove", firstCartProductModel);

            string cartCookieValue = string.Empty;
            var updatedCookieHeaderAfterRemoving = removeResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (updatedCookieHeaderAfterRemoving != null)
            {
                cartCookieValue = updatedCookieHeaderAfterRemoving.Split(';').FirstOrDefault()!.Split('=').Last();
            }
            var decodedCartCookieValue = HttpUtility.UrlDecode(cartCookieValue);

            //Parse the cart cookie value into CartServiceModel
            var cartServiceModel = JsonSerializer.Deserialize<CartServiceModel>(decodedCartCookieValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new CartServiceModel();

            // Assert
            Assert.Equal(HttpStatusCode.OK, removeResponse.StatusCode);
            Assert.Equal(0, cartServiceModel.TotalSaved);
            Assert.Equal(0, cartServiceModel.TotalProducts);
            Assert.Equal(0, cartServiceModel.OriginalPrice);
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
