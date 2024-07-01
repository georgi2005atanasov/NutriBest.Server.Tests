using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Brands
{
    using Xunit;
    using System.Net;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Brands.Models;
    using Infrastructure.Extensions;
    using NutriBest.Server.Features.Products.Models;
    using NutriBest.Server.Features.Promotions.Models;
    using static ErrorMessages.BrandsController;
    using System.Text.Json;
    using NutriBest.Server.Shared.Responses;

    [Collection("Brands Controller Tests")]
    public class RemoveBrandIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public RemoveBrandIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Theory]
        [InlineData("Goshoolu", 5, "The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_ForAdmin(string name,
            int index, string description)
        {
            // Arrange
            var uniqueName = $"{name}{index}";

            var client = await clientHelper.GetAdministratorClientAsync();
            var brandModel = new CreateBrandServiceModel
            {
                Description = description,
                Name = uniqueName
            };
            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };
            await client.PostAsync("/Brands", formData);

            // Act
            var response = await client.DeleteAsync($"/Brands/{uniqueName}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("Goshoolu", 6, "The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_ForEmployee(string name,
            int index ,string description)
        {
            // Arrange
            var uniqueName = $"{name}{index}";

            var client = await clientHelper.GetEmployeeClientAsync();
            var brandModel = new CreateBrandServiceModel
            {
                Description = description,
                Name = uniqueName
            };
            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };
            await client.PostAsync("/Brands", formData);

            // Act
            var response = await client.DeleteAsync($"/Brands/{uniqueName}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("Goshoolu", 7, "The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_AndAlsoShouldDeleteProduct(string name,
            int index, string description)
        {
            // Arrange
            var uniqueName = $"{name}{index}";

            var client = await clientHelper.GetEmployeeClientAsync();
            var brandModel = new CreateBrandServiceModel
            {
                Description = description,
                Name = uniqueName
            };
            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };
            await client.PostAsync("/Brands", formData);
            await SeedingHelper.SeedProduct(clientHelper, productName: "product300", uniqueName);

            Assert.NotEmpty(db!.Products);

            // Act
            var response = await client.DeleteAsync($"/Brands/{uniqueName}");

            // Assert
            Assert.Empty(db.Products);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("Goshoolu", 9, "The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_AndAlsoShouldDeletePromotion(string name,
            int index, string description)
        {
            // Arrange
            var uniqueName = $"{name}{index}";

            var client = await clientHelper.GetEmployeeClientAsync();
            var brandModel = new CreateBrandServiceModel
            {
                Description = description,
                Name = uniqueName
            };
            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };
            await client.PostAsync("/Brands", formData);
            await SeedingHelper.SeedPromotion(clientHelper, uniqueName);

            Assert.NotEmpty(db!.Promotions);

            // Act
            var response = await client.DeleteAsync($"/Brands/{uniqueName}");

            // Assert
            Assert.Empty(db.Promotions);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("Goshoolu", 10, "The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_AndAlsoShouldDeleteProductAndPromotion(string name,
            int index, string description)
        {
            // Arrange
            var uniqueName = $"{name}{index}";

            var client = await clientHelper.GetEmployeeClientAsync();
            var brandModel = new CreateBrandServiceModel
            {
                Description = description,
                Name = uniqueName
            };
            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };
            await client.PostAsync("/Brands", formData);
            await SeedingHelper.SeedProduct(clientHelper, productName: "product301", uniqueName);
            await SeedingHelper.SeedPromotion(clientHelper, uniqueName);

            Assert.NotEmpty(db!.Products);
            Assert.NotEmpty(db!.Promotions);

            // Act
            var response = await client.DeleteAsync($"/Brands/{uniqueName}");

            // Assert
            Assert.Empty(db.Products);
            Assert.Empty(db.Promotions);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RemoveBrand_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.DeleteAsync($"/Brands/NoNeedForName");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RemoveBrand_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.DeleteAsync($"/Brands/NoNeedForName");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RemoveBrand_ShouldReturnBadRequest_WhenBrandDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.DeleteAsync($"/Brands/InvalidName");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidBrandName, result.Message);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedBrands();
            db.SeedCategories();
            db.SeedPackages();
            db.SeedFlavours();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
