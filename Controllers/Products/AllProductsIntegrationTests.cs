namespace NutriBest.Server.Tests.Controllers.Products
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Products.Models;
    using Infrastructure.Extensions;

    [Collection("Products Controller Tests")]
    public class AllProductsIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllProductsIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithoutFilters()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithPriceFilterDesc()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1&price=desc");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(3, result.Count);
            Assert.Equal(db.Products
                .Select(x => x.StartingPrice)
                .OrderByDescending(x => x),
                result.ProductsRows![0]
                .Select(x => x.Price));
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithPriceFilterAsc()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1&price=asc");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(3, result.Count);
            Assert.Equal(db.Products
                .OrderBy(x => x.StartingPrice)
                .Select(x => x.StartingPrice),
                result.ProductsRows![0]
                .Select(x => x.Price));
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithAlphaFilterDesc()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1&alpha=desc");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(3, result.Count);
            Assert.Equal(db.Products
                .OrderByDescending(x => x.Name)
                .Select(x => x.StartingPrice),
                result.ProductsRows![0]
                .Select(x => x.Price));
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithAlphaFilterAsc()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1&alpha=asc");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(3, result.Count);
            Assert.Equal(db.Products
                .OrderBy(x => x.Name)
                .Select(x => x.StartingPrice),
                result.ProductsRows![0]
                .Select(x => x.Price));
        }

        [Theory]
        [InlineData("Nordic Naturals", 1)]
        [InlineData("Klean Athlete", 2)]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithChosenBrand(string chosenBrand,
            int expectedCount)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync($"/Products?page=1&brand={chosenBrand}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithChosenCategories()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1&categories=Vitamins and Creatines");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(2, result.Count);
            Assert.Equal(db.Products
                .Where(x => x.ProductsCategories
                .Any(x => x.Category.Name == "Vitamins" || x.Category.Name == "Creatines"))
                .Select(x => x.Name)
                .OrderBy(x => x),
                result.ProductsRows![0]
                .Select(x => x.Name)
                .OrderBy(x => x));
        }

        [Theory]
        [InlineData("product", 3)]
        [InlineData("product71", 1)]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithTypedSearch(string search,
            int expectedCount)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync($"/Products?page=1&search={search}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithChosenProductsViewTable()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1&productsView=table");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, result.ProductsRows!.Count);
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithValidPagination()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);
            await SeedingHelper.SeedSevenProducts(clientHelper);

            Assert.Equal(10, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=2");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, result.ProductsRows![0].Count);
            Assert.True(result.ProductsRows![1].Count == 1);
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithValidPagination_ForTable()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);
            await SeedingHelper.SeedSevenProducts(clientHelper);

            Assert.Equal(10, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=2&productsView=table");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(result.ProductsRows);
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldReturnBadRequest_ForUsersWithChosenProductsViewTable()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1&productsView=table");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldReturnBadRequest_ForAnonymousWithChosenProductsViewTable()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1&productsView=table");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Theory]
        [InlineData("10 16", 1)]
        [InlineData("10 60", 2)]
        [InlineData("10 100", 3)]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithPriceRange(string priceRange,
            int expectedCount)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync($"/Products?page=1&priceRange={priceRange}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(expectedCount, result.Count);
        }

        [Theory]
        [InlineData("250", 1)]
        [InlineData("250 500", 2)]
        [InlineData("1000 500 250", 3)]
        [InlineData("1500", 0)]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithGivenQuantities(string quantities,
            int expectedCount)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync($"/Products?page=1&quantities={quantities}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(expectedCount, result.Count);
        }

        [Theory]
        [InlineData("Coconut", 1)]
        [InlineData("Coconut andAlso Cookies and Cream", 2)]
        [InlineData("Lemon Lime andAlso Cookies and Cream andAlso Coconut", 3)]
        [InlineData("Banana andAlso Chocolate", 0)]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithGivenFlavours(string flavours,
            int expectedCount)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync($"/Products?page=1&flavours={flavours}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(expectedCount, result.Count);
        }

        [Theory]
        [MemberData(nameof(ProductFilterTestData.FilterCombinations), MemberType = typeof(ProductFilterTestData))]
        public async Task AllProductsEndpoint_ShouldBeExecuted_WithMultipleFilters(
        string? priceOrder,
        string? alphaOrder,
        string? brand,
        string? categories,
        string? search,
        string? quantities,
        string? priceRange,
        int expectedCount)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            Assert.Equal(7, db!.Products.Count());

            // Act
            var url = "/Products?page=1";

            if (!string.IsNullOrEmpty(priceOrder))
                url += $"&price={priceOrder}";
            if (!string.IsNullOrEmpty(alphaOrder))
                url += $"&alpha={alphaOrder}";
            if (!string.IsNullOrEmpty(brand))
                url += $"&brand={brand}";
            if (!string.IsNullOrEmpty(categories))
                url += $"&categories={categories}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={search}";
            if (!string.IsNullOrEmpty(quantities))
                url += $"&quantities={quantities}";
            if (!string.IsNullOrEmpty(priceRange))
                url += $"&priceRange={priceRange}";

            var response = await client.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            Assert.Equal(expectedCount, result.Count);
        }

        [Fact]
        public async Task AllProductsEndpoint_ShouldReturnProductsWithPromotions_RegardlessOfTheAppliedPriceRange()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var (formDataPercentDiscount, formDataAmountDiscount) = SeedingHelper.GetTwoPromotions();

            // Act
            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            await client.PostAsync("/Promotions", formDataAmountDiscount);
            await client.PutAsync("/Promotions/Status/2", null);

            var url = "/Products?page=1&priceRange=1 5";

            var response = await client.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProductsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProductsServiceModel();

            decimal product77 = (decimal)result
                        .ProductsRows!
                        .SelectMany(x => x)
                        .First(x => x.Name == "product77").DiscountPercentage!;

            Assert.Equal(3, result.Count);
            Assert.Equal(25.00m, Math.Round((decimal)result
                        .ProductsRows!
                        .SelectMany(x => x)
                        .First(x => x.Name == "product77").DiscountPercentage!, 2));
            Assert.Equal(62.54m, Math.Round((decimal)result
                        .ProductsRows!
                        .SelectMany(x => x)
                        .First(x => x.Name == "product80").DiscountPercentage!, 2));
            Assert.Equal(10.00m, Math.Round((decimal)result
                        .ProductsRows!
                        .SelectMany(x => x)
                        .First(x => x.Name == "product81").DiscountPercentage!, 2));
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
