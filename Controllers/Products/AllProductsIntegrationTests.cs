using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Products
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Products.Models;
    using Infrastructure.Extensions;
    using System.Net;

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

            await SeedProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            var response = await client.GetAsync("/Products?page=1");
            var data = await response.Content.ReadAsStringAsync();
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

            await SeedProducts(clientHelper);

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

            await SeedProducts(clientHelper);

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

            await SeedProducts(clientHelper);

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

            await SeedProducts(clientHelper);

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

            await SeedProducts(clientHelper);

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

            await SeedProducts(clientHelper);

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

            await SeedProducts(clientHelper);

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

            await SeedProducts(clientHelper);

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
        public async Task AllProductsEndpoint_ShouldReturnBadRequest_ForUsersWithChosenProductsViewTable()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedProducts(clientHelper);

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

            await SeedProducts(clientHelper);

            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.GetAsync("/Products?page=1&productsView=table");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        private static async Task SeedProducts(ClientHelper clientHelper)
        {
            await SeedingHelper.SeedProduct(clientHelper,
                "product71",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"15.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product72",
                new List<string>
                {
                    "Vitamins"
                },
                "100",
                "Klean Athlete",
                "[{ \"flavour\": \"Cookies and Cream\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product73",
                            new List<string>
                {
                    "Proteins"
                },
                "10",
                "Nordic Naturals",
                "[{ \"flavour\": \"Lemon Lime\", \"grams\": 500, \"quantity\": 100, \"price\": \"50.99\"}]");
        }
    }
}
