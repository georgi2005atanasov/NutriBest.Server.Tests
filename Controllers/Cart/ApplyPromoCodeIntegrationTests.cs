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
    public class ApplyPromoCodeIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public ApplyPromoCodeIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task ApplyPromoCode_ShouldBeExecuted()
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

            var responseAfterPromoCode = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
            var updatedCookieHeaderAfterPromoCode = responseAfterPromoCode
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();
            string cartCookieValue = string.Empty;
            if (updatedCookieHeaderAfterPromoCode != null)
            {
                cartCookieValue = updatedCookieHeaderAfterPromoCode
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
            Assert.Equal(1582.296m, cartServiceModel.TotalProducts);
            Assert.Equal(395.574m, cartServiceModel.TotalSaved);
        }

        [Fact]
        public async Task ApplyPromoCode_ShouldBeExecuted_AndShouldAlsoBeValidAfterAddingMoreProducts()
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
            var secondResponse = await client.PostAsJsonAsync("/Cart/Set", secondCartProductModel);
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
            var thirdResponse = await client.PostAsJsonAsync("/Cart/Set", thirdCartProductModel);
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

            var responseAfterPromoCode = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
            var updatedCookieHeaderAfterPromoCode = responseAfterPromoCode
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterPromoCode != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterPromoCode);
            }

            thirdCartProductModel.Count = 1;
            var fourthResponse = await client.PostAsJsonAsync("/Cart/Add", thirdCartProductModel);
            var updatedCookieHeaderAfterFourth = fourthResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();
            if (updatedCookieHeaderAfterFourth != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterFourth);
            }

            string cartCookieValue = string.Empty;
            if (updatedCookieHeaderAfterFourth != null)
            {
                cartCookieValue = updatedCookieHeaderAfterFourth
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

            Assert.Equal(2478.86m, cartServiceModel.OriginalPrice);
            Assert.Equal(1983.088m, cartServiceModel.TotalProducts);
            Assert.Equal(495.772m, cartServiceModel.TotalSaved);
        }

        [Fact]
        public async Task ApplyPromoCode_ShouldBeExecuted_AndShouldAlsoBeValidAfterRemovingProduct()
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
            var secondResponse = await client.PostAsJsonAsync("/Cart/Set", secondCartProductModel);
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
            var thirdResponse = await client.PostAsJsonAsync("/Cart/Set", thirdCartProductModel);
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

            var responseAfterPromoCode = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
            var updatedCookieHeaderAfterPromoCode = responseAfterPromoCode
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterPromoCode != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterPromoCode);
            }

            thirdCartProductModel.Count = 1;
            var fourthResponse = await client.PostAsJsonAsync("/Cart/Remove", thirdCartProductModel);
            var updatedCookieHeaderAfterFourth = fourthResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();
            if (updatedCookieHeaderAfterFourth != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterFourth);
            }

            string cartCookieValue = string.Empty;
            if (updatedCookieHeaderAfterFourth != null)
            {
                cartCookieValue = updatedCookieHeaderAfterFourth
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

            Assert.Equal(1476.88m, cartServiceModel.OriginalPrice);
            Assert.Equal(1181.504m, cartServiceModel.TotalProducts);
            Assert.Equal(295.376m, cartServiceModel.TotalSaved);
        }

        [Fact]
        public async Task ApplyPromoCode_ShouldBeExecuted_AndShouldAlsoBeValidAfterSettingProduct()
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
            var secondResponse = await client.PostAsJsonAsync("/Cart/Set", secondCartProductModel);
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
            var thirdResponse = await client.PostAsJsonAsync("/Cart/Set", thirdCartProductModel);
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

            var responseAfterPromoCode = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
            var updatedCookieHeaderAfterPromoCode = responseAfterPromoCode
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterPromoCode != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterPromoCode);
            }

            thirdCartProductModel.Count = 10;
            var fourthResponse = await client.PostAsJsonAsync("/Cart/Set", thirdCartProductModel);
            var updatedCookieHeaderAfterFourth = fourthResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();
            if (updatedCookieHeaderAfterFourth != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterFourth);
            }

            string cartCookieValue = string.Empty;
            if (updatedCookieHeaderAfterFourth != null)
            {
                cartCookieValue = updatedCookieHeaderAfterFourth
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

            Assert.Equal(5484.80m, cartServiceModel.OriginalPrice);
            Assert.Equal(4387.840m, cartServiceModel.TotalProducts);
            Assert.Equal(1096.960m, cartServiceModel.TotalSaved);
        }

        [Fact]
        public async Task ApplyPromoCode_ShouldBeExecuted_AndShouldAlsoBeValidAfterRemovingAllProducts()
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
            var cookieHeader = firstResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            await SeedingHelper.SeedPromoCode(clientHelper,
                "20% OFF!",
                "1",
                "20");

            var promoCodeModel = new ApplyPromoCodeServiceModel
            {
                Code = db!.PromoCodes.First().Code
            };

            var responseAfterPromoCode = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
            var updatedCookieHeaderAfterPromoCode = responseAfterPromoCode
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterPromoCode != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterPromoCode);
            }

            var fourthResponse = await client.PostAsJsonAsync("/Cart/Remove", firstCartProductModel);
            var updatedCookieHeaderAfterDeletion = fourthResponse
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();
            if (updatedCookieHeaderAfterDeletion != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterDeletion);
            }

            string cartCookieValue = string.Empty;
            if (updatedCookieHeaderAfterDeletion != null)
            {
                cartCookieValue = updatedCookieHeaderAfterDeletion
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

            Assert.Equal(0m, cartServiceModel.OriginalPrice);
            Assert.Equal(0m, cartServiceModel.TotalProducts);
            Assert.Equal(0m, cartServiceModel.TotalSaved);
        }

        [Fact]
        public async Task ApplyPromoCode_ShouldBeExecuted_AfterAlreadyHavingAnotherPromoCode()
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

            await SeedingHelper.SeedPromoCode(clientHelper,
                "10% OFF!",
                "1",
                "10");

            var promoCodeModel = new ApplyPromoCodeServiceModel
            {
                Code = db!.PromoCodes
                       .First(x => x.Description == "20% OFF!")
                       .Code
            };

            var responseAfterPromoCode = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
            var updatedCookieHeaderAfterPromoCode = responseAfterPromoCode
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            if (updatedCookieHeaderAfterPromoCode != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders
                    .Add("Cookie", updatedCookieHeaderAfterPromoCode);
            }

            promoCodeModel.Code = db!.PromoCodes
                                  .First(x => x.Description == "10% OFF!")
                                  .Code;

            var responseAfterSecondPromoCode = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
            var updatedCookieHeaderAfterSecondPromoCode = responseAfterSecondPromoCode
                .Headers
                .GetValues("Set-Cookie")
                .FirstOrDefault();

            string cartCookieValue = string.Empty;
            if (updatedCookieHeaderAfterSecondPromoCode != null)
            {
                cartCookieValue = updatedCookieHeaderAfterSecondPromoCode
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
            Assert.Equal(1780.083m, cartServiceModel.TotalProducts);
            Assert.Equal(197.787m, cartServiceModel.TotalSaved);
        }

        [Fact]
        public async Task ApplyPromoCode_ShouldReturnBadRequest_WhenThereAreNoProducts()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedPromoCode(clientHelper,
                "20% OFF!",
                "1",
                "20");

            var promoCodeModel = new ApplyPromoCodeServiceModel
            {
                Code = db!.PromoCodes.First().Code
            };

            // Act
            var response = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(YouHaveToAddProducts, result.Message);
        }

        [Fact]
        public async Task ApplyPromoCode_ShouldReturnBadRequest_WhenPromoCodeIsInvalid()
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

            var responseAfterPromoCode = await client.PostAsJsonAsync("/Cart/ApplyPromoCode", promoCodeModel);
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
