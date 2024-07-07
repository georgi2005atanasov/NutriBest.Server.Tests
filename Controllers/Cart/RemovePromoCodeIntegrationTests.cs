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
    using static ErrorMessages.PromoCodeController;

    [Collection("Cart Controller Tests")]
    public class RemovePromoCodeIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public RemovePromoCodeIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task RemovePromoCode_ShouldBeExecuted()
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
            var cookieHeader = firstResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Second product addition
            var secondResponse = await client.PostAsJsonAsync("/Cart/Add", secondCartProductModel);
            var updatedCookieHeaderAfterSecond = secondResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterSecond != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterSecond);
            }

            // Third product addition
            var thirdResponse = await client.PostAsJsonAsync("/Cart/Add", thirdCartProductModel);
            var updatedCookieHeaderAfterThird = thirdResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterThird != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterThird);
            }

            await SeedingHelper.SeedPromoCode(clientHelper,
                "20% OFF!",
                "1",
                "20");

            var promoCodeModel = new ApplyPromoCodeServiceModel
            {
                Code = db!.PromoCodes.First().Code
            };

            var responseAfterApplyPromoCode = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
            var updatedCookieHeaderAfterApplyPromoCode = responseAfterApplyPromoCode
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterApplyPromoCode != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterApplyPromoCode);
            }

            var responseAfterRemovePromoCode = await client.PostAsJsonAsync("/Cart/RemovePromoCode", promoCodeModel);
            var updatedCookieHeaderAfterRemovePromoCode = responseAfterRemovePromoCode
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            string cartCookieValue = string.Empty;
            if (updatedCookieHeaderAfterRemovePromoCode != null)
            {
                cartCookieValue = updatedCookieHeaderAfterRemovePromoCode
                    .Split(';')
                    .FirstOrDefault()!
                    .Split('=')
                    .Last();
            }

            var decodedCartCookieValue = HttpUtility.UrlDecode(cartCookieValue);

            // Assert
            var cartServiceModel = JsonSerializer.Deserialize<CartServiceModel>(decodedCartCookieValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new CartServiceModel();

            Assert.Equal(1977.87m, cartServiceModel.OriginalPrice);
            Assert.Equal(1977.87m, cartServiceModel.TotalProducts);
            Assert.Equal(0, cartServiceModel.TotalSaved);
            Assert.Equal("", cartServiceModel.Code);
        }

        [Fact]
        public async Task RemovePromoCode_ShouldReturnBadRequest_WhenPromoCodeIsInvalid()
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
            var cookieHeader = firstResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Second product addition
            var secondResponse = await client.PostAsJsonAsync("/Cart/Add", secondCartProductModel);
            var updatedCookieHeaderAfterSecond = secondResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterSecond != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterSecond);
            }

            // Third product addition
            var thirdResponse = await client.PostAsJsonAsync("/Cart/Add", thirdCartProductModel);
            var updatedCookieHeaderAfterThird = thirdResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterThird != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterThird);
            }

            await SeedingHelper.SeedPromoCode(clientHelper,
                "20% OFF!",
                "1",
                "20");

            var promoCodeModel = new ApplyPromoCodeServiceModel
            {
                Code = "FAKECOD"
            };

            var responseAfterPromoCode = await client.PostAsJsonAsync("/Cart/RemovePromoCode", promoCodeModel);
            var data = await responseAfterPromoCode.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, responseAfterPromoCode.StatusCode);
            Assert.Equal(InvalidPromoCode, result.Message);
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
