using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Brands
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Brands.Models;
    using Infrastructure.Extensions;
    using System.Net;
    using System.Text.Json;
    using NutriBest.Server.Shared.Responses;
    using static ErrorMessages.BrandsController;

    [Collection("Brands Controller Tests")]
    public class CreateBrandIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CreateBrandIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task CreateBrand_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var brandModel = new CreateBrandServiceModel
            {
                Description = "The Gosho Brand",
                Name = Guid.NewGuid().ToString()
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };

            // Act
            var response = await client.PostAsync("/Brands", formData);
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateBrand_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var brandModel = new CreateBrandServiceModel
            {
                Description = "The Gosho Brand",
                Name = Guid.NewGuid().ToString()
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };

            // Act
            var response = await client.PostAsync("/Brands", formData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CreateBrand_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var brandModel = new CreateBrandServiceModel
            {
                Description = "The Gosho Brand",
                Name = "Goshoolu"
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };

            // Act
            var response = await client.PostAsync("/Brands", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateBrand_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var brandModel = new CreateBrandServiceModel
            {
                Description = "The Gosho Brand",
                Name = "Goshoolu"
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };

            // Act
            var response = await client.PostAsync("/Brands", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateBrand_ShouldReturnBadRequest_WhenBrandExists()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var brandModel = new CreateBrandServiceModel
            {
                Description = "The Gosho Brand",
                Name = "Goshoolu3"
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };

            // Act
            await client.PostAsync("/Brands", formData);
            var response = await client.PostAsync("/Brands", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(BrandAlreadyExists, result.Message);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedBrands();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
