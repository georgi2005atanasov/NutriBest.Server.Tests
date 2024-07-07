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
    public class AddToCartIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AddToCartIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AddToCart_ShouldBeExecuted_AndShouldAlsoSetCookie()
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
            var response = await client.PostAsJsonAsync("/Cart/Add", cartProductModel);
            var cookies = response.Headers.GetValues("Set-Cookie");
            var shoppingCartCookie = cookies.FirstOrDefault(cookie => cookie.StartsWith("ShoppingCart="));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(shoppingCartCookie);
        }

        [Fact]
        public async Task AddToCart_ShouldBeExecuted_AndShouldAlsoMakeValidCalculations()
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
            string cartCookieValue = string.Empty;
            if (updatedCookieHeaderAfterThird != null)
            {
                cartCookieValue = updatedCookieHeaderAfterThird.Split(';').FirstOrDefault()!.Split('=').Last();
            }

            var decodedCartCookieValue = HttpUtility.UrlDecode(cartCookieValue);

            // Parse the cart cookie value into CartServiceModel
            var cartServiceModel = JsonSerializer.Deserialize<CartServiceModel>(decodedCartCookieValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Assert
            Assert.NotNull(cartServiceModel);
            Assert.Equal(3, cartServiceModel!.CartProducts.Count);
            Assert.Equal(1977.87m, cartServiceModel.TotalProducts); // Adjust the expected value based on your logic
            Assert.Equal(1977.87m, cartServiceModel.OriginalPrice); // Adjust the expected value based on your logic
            Assert.Equal(0.0m, cartServiceModel.TotalSaved); // Adjust the expected value based on your logic
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
                Count = -1,
                Price = 15.99m,
                ProductId = 1
            };

            // Act
            var response = await client.PostAsJsonAsync("/Cart/Add", cartProductModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidProductCount, result.Message);
        }

        [Fact]
        public async Task AddToCart_ShouldReturnBadRequest_WhenProductPackageFlavourIsInvalid()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var cartProductModel = new CartProductServiceModel
            {
                Flavour = "CoconutPesho",
                Grams = 1234,
                Count = 5,
                Price = 150.99m,
                ProductId = 1
            };

            // Act
            var response = await client.PostAsJsonAsync("/Cart/Add", cartProductModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ProductDoesNotExists, result.Message);
        }

        [Fact]
        public async Task AddToCart_ShouldReturnBadRequest_WhenCountIsLessThanAvailable()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var cartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 101,
                Price = 15.99m,
                ProductId = 1
            };

            // Act
            var response = await client.PostAsJsonAsync("/Cart/Add", cartProductModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(string.Format(ProductsAreNotAvailableWithThisCount, 100), result.Message);
        }

        [Fact]
        public async Task AddToCart_ShouldReturnBadRequest_WhenCountIsLessThanAvailable_OnSecondAdd()
        {
            // Arrange
            var client = fixture.Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            });

            var authToken = Encoding.ASCII.GetBytes("username:password");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));

            var firstCartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 100,
                Price = 15.99m,
                ProductId = 1
            };
            var secondCartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 1,
                Price = 15.99m,
                ProductId = 1
            };

            // Act
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            firstResponse.EnsureSuccessStatusCode();

            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            var secondResponse = await client.PostAsJsonAsync("/Cart/Add", secondCartProductModel);
            var data = await secondResponse.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
            Assert.Equal(string.Format(ProductsAreNotAvailableWithThisCount, 100), result.Message);
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
