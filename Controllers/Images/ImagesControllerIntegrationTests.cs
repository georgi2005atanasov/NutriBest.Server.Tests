namespace NutriBest.Server.Tests.Controllers.Images
{
    using System.Net.Http.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using Infrastructure.Extensions;
    using System.Text.Json;
    using NutriBest.Server.Features.Images.Models;
    using Microsoft.Extensions.Caching.Memory;
    using System.Net;

    [Collection("Images Controller Tests")]
    public class ImagesControllerIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        private IMemoryCache? cache;

        public ImagesControllerIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetImageByProductId_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Images/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.NotNull(response);
            var result = JsonSerializer.Deserialize<ImageListingServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ImageListingServiceModel();
            Assert.Equal("image/png", result.ContentType);
        }

        [Fact]
        public async Task GetImageByProductId_ShouldCacheTheImage()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Images/1");

            // Assert
            Assert.True(cache!.TryGetValue("product_image_1", out _));
        }

        [Fact]
        public async Task GetImageByProductId_ShouldReturnBadRequest_WhenProductIsInvalid()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            // Act
            var response = await client.GetAsync("/Images/123");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetImageByBrandId_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedBrand(clientHelper, 
                "UniqueBrand123",
                "Some unique brand");

            // Act
            var response = await client.GetAsync("/Images/Brand/UniqueBrand123");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = JsonSerializer.Deserialize<ImageListingServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ImageListingServiceModel();
            Assert.Equal("image/png", result.ContentType);
        }

        [Fact]
        public async Task GetImageByBrandId_ShouldCacheTheImage()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedBrand(clientHelper,
                "UniqueBrand123",
                "Some unique brand");

            // Act
            var response = await client.GetAsync("/Images/Brand/UniqueBrand123");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(cache!.TryGetValue("brand_logo_image_UniqueBrand123", out _));
        }

        [Fact]
        public async Task GetImageByBrandId_ShouldReturnBadRequest_WhenBrandIsInvalid()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Images/Brand/InvalidBrandName123");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
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
