using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Products
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Products.Models;
    using Infrastructure.Extensions;
    using static ErrorMessages.ProductsController;

    [Collection("Products Controller Tests")]
    public class PatchProductIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public PatchProductIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task PatchProduct_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var productUpdateModel = new PartialUpdateProductServiceModel
            {
                Description = "Some new description"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(productUpdateModel.Description), "Description" }
            };

            // Act
            var response = await client.PatchAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("1", data);
        }

        [Fact]
        public async Task PatchProduct_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var productUpdateModel = new PartialUpdateProductServiceModel
            {
                Description = "Some new description"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(productUpdateModel.Description), "Description" }
            };

            // Act
            var response = await client.PatchAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("1", data);
        }

        [Fact]
        public async Task PatchProduct_ShouldReturnBadRequest_WhenProductDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var productUpdateModel = new PartialUpdateProductServiceModel
            {
                Description = "Some new description"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(productUpdateModel.Description), "Description" }
            };

            // Act
            var response = await client.PatchAsync("/Products/100", formData);
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
        public async Task PatchProduct_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var productUpdateModel = new PartialUpdateProductServiceModel
            {
                Description = "Some new description"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(productUpdateModel.Description), "Description" }
            };

            // Act
            var response = await client.PatchAsync("/Products/1", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PatchProduct_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var productUpdateModel = new PartialUpdateProductServiceModel
            {
                Description = "Some new description"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(productUpdateModel.Description), "Description" }
            };

            // Act
            var response = await client.PatchAsync("/Products/1", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
