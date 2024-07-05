namespace NutriBest.Server.Tests.Controllers.Products
{
    using System.Net;
    using Xunit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using Infrastructure.Extensions;

    [Collection("Products Controller Tests")]
    public class DeleteProductIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public DeleteProductIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task DeleteProduct_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);
            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.DeleteAsync("/Products/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);
            Assert.Equal(2, db!.Products.Count());
            Assert.True(db.Products
                .IgnoreQueryFilters()
                .First(x => x.ProductId == 1)
                .IsDeleted);

            Assert.True(db.ProductsPackagesFlavours
                .IgnoreQueryFilters()
                .First(x => x.ProductId == 1)
                .IsDeleted);

            Assert.True(db.ProductsCategories
                .IgnoreQueryFilters()
                .First(x => x.ProductId == 1)
                .IsDeleted);

            Assert.True(db.ProductsDetails
                .IgnoreQueryFilters()
                .First(x => x.ProductId == 1)
                .IsDeleted);
        }

        [Fact]
        public async Task DeleteProduct_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);
            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.DeleteAsync("/Products/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);
            Assert.Equal(2, db!.Products.Count());
            Assert.True(db.Products
                .IgnoreQueryFilters()
                .First(x => x.ProductId == 1)
                .IsDeleted);

            Assert.True(db.ProductsPackagesFlavours
                .IgnoreQueryFilters()
                .First(x => x.ProductId == 1)
                .IsDeleted);

            Assert.True(db.ProductsCategories
                .IgnoreQueryFilters()
                .First(x => x.ProductId == 1)
                .IsDeleted);

            Assert.True(db.ProductsDetails
                .IgnoreQueryFilters()
                .First(x => x.ProductId == 1)
                .IsDeleted);
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnBadRequest_WhenProductDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);
            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.DeleteAsync("/Products/100");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(3, db!.Products.Count());
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedThreeProducts(clientHelper);
            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.DeleteAsync("/Products/1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(3, db!.Products.Count());
        }

        [Fact]
        public async Task DeleteProduct_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);
            Assert.Equal(3, db!.Products.Count());

            // Act
            var response = await client.DeleteAsync("/Products/1");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal(3, db!.Products.Count());
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
