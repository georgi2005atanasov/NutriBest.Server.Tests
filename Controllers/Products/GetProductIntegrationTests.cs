using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Products
{
    using System.Net;
    using System.Text.Json;
    using System.Net.Http.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Products.Models;
    using NutriBest.Server.Features.Categories.Models;
    using NutriBest.Server.Infrastructure.Extensions;
    using static ErrorMessages.ProductsController;
    using static ErrorMessages.PromotionsController;

    [Collection("Products Controller Tests")]
    public class GetProductIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public GetProductIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetProductById_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Products/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProductServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProductServiceModel();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("product71", result.Name);
            Assert.Single(result.Categories);
            Assert.Contains("Creatines", result.Categories);
            Assert.Equal("Klean Athlete", result.Brand);
            Assert.Equal("this is product 1", result.Description);
        }

        [Fact]
        public async Task GetProductById_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Products/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<ProductServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProductServiceModel();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("product71", result.Name);
            Assert.Single(result.Categories);
            Assert.Contains("Creatines", result.Categories);
            Assert.Equal("Klean Athlete", result.Brand);
            Assert.Equal("this is product 1", result.Description);
        }

        //InvalidProduct
        [Fact]
        public async Task GetProductById_ShouldReturnBadRequest_WhenProductDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Products/21");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidProduct, result.Message);
        }

        [Fact]
        public async Task GetProductById_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Products/1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetProductById_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Products/1");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GetProductsIds_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Products/Identifiers");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<int>>(data);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, result!.Count());
        }

        [Fact]
        public async Task GetProductsByCategoryCount_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Products/ByCategoryCount");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<IEnumerable<CategoryCountServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CategoryCountServiceModel>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(12, result!.Count());
            Assert.Equal(1, result
                        .First(x => x.Category == "Amino Acids")
                        .Count);
            Assert.Equal(1, result
                        .First(x => x.Category == "Creatines")
                        .Count);
            Assert.Equal(1, result
                        .First(x => x.Category == "Fish Oils")
                        .Count);
            Assert.Equal(2, result
                        .First(x => x.Category == "Vitamins")
                        .Count);
            Assert.Equal(2, result
                        .First(x => x.Category == "Proteins")
                        .Count);
            Assert.Equal(0, result
                        .First(x => x.Category == "Vegan")
                        .Count);
            Assert.Equal(0, result
                        .First(x => x.Category == "Post-Workout")
                        .Count);
            Assert.Equal(0, result
                        .First(x => x.Category == "Pre-Workout")
                        .Count);
            Assert.Equal(0, result
                        .First(x => x.Category == "Promotions")
                        .Count);
            Assert.Equal(0, result
                        .First(x => x.Category == "Fat Burners")
                        .Count);
            Assert.Equal(0, result
                        .First(x => x.Category == "Recovery")
                        .Count);
            Assert.Equal(0, result
                        .First(x => x.Category == "Mass Gainers")
                        .Count);
        }

        [Fact]
        public async Task GetWithPromotion_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();
            await SeedingHelper.SeedThreeProducts(clientHelper);

            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            // Act
            var response = await client.GetAsync("/Products/1/7");
            var data = await response.Content.ReadAsStringAsync();
            
            // Assert
            var result = JsonSerializer.Deserialize<ProductWithPromotionDetailsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProductWithPromotionDetailsServiceModel();
            Assert.Equal(7, result.ProductId);
            Assert.Equal(1500.74m, Math.Round(result.NewPrice, 2));
            Assert.Equal(25, result.DiscountPercentage);
        }

        [Fact]
        public async Task GetWithPromotion_ShouldReturnBadRequest_WhenPromotionNotFound()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();
            await SeedingHelper.SeedThreeProducts(clientHelper);

            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            // Act
            var response = await client.GetAsync("/Products/9/7");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidPromotion, result.Message);
        }

        [Fact]
        public async Task GetWithPromotion_ShouldReturnBadRequest_WhenProductNotFound()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();
            await SeedingHelper.SeedThreeProducts(clientHelper);

            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            // Act
            var response = await client.GetAsync("/Products/1/900");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidProduct, result.Message);
        }

        [Fact]
        public async Task GetCurrentPrice_ShoouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var productModel = new CurrentProductPriceServiceModel
            {
                ProductId = 1,
                Flavour = "Coconut",
                Package = 1000
            };

            // Act
            var response = await client.PostAsJsonAsync("/Products/CurrentPriceWithQuantity",
                productModel);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProductPriceQuantityServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProductPriceQuantityServiceModel();

            // Assert
            Assert.Equal(15.99m, result.Price);
            Assert.Equal(100, result.Quantity);
        }

        [Fact]
        public async Task GetRelatedProductsByCategory_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var model = new RelatedProductsServiceModel
            {
                ProductId = 4,
                Categories = new List<string>
                {
                    "Proteins"
                }
            };

            // Act
            var response = await client.PostAsJsonAsync("/Products/Related", model);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<ProductListingServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ProductListingServiceModel>();
            Assert.Single(result);
        }

        [Fact]
        public async Task GetRelatedProductsByCategory_ShouldBeExecuted_AndReturnEmpty()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var model = new RelatedProductsServiceModel
            {
                ProductId = 5,
                Categories = new List<string>
                {
                    "Amino Acids"
                }
            };

            // Act
            var response = await client.PostAsJsonAsync("/Products/Related", model);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<ProductListingServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ProductListingServiceModel>();
            Assert.Empty(result);
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
