using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Brands
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Brands.Models;
    using Infrastructure.Extensions;
    using static ErrorMessages.BrandsController;

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
        [InlineData("The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_ForAdmin(string description)
        {
            // Arrange
            var uniqueName = Guid.NewGuid().ToString();

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
            Assert.True(!db!.Brands.Any(x => x.Name == uniqueName));
        }

        [Theory]
        [InlineData("The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_ForEmployee(string description)
        {
            // Arrange
            var uniqueName = Guid.NewGuid().ToString();

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
            Assert.True(!db!.Brands.Any(x => x.Name == uniqueName));
        }

        [Theory]
        [InlineData("The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_AndAlsoShouldDeleteProduct(string description)
        {
            // Arrange
            var uniqueName = Guid.NewGuid().ToString();

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
            await SeedingHelper.SeedProduct(clientHelper,
                "product10",
                            new List<string>
                {
                    "Creatines"
                },
                "300",
                uniqueName,
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            Assert.True(db!.Products
                        .Any(x => x.Brand != null && x.Brand.Name == uniqueName));

            // Act
            var response = await client.DeleteAsync($"/Brands/{uniqueName}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db.Products);
        }

        [Theory]
        [InlineData("The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_AndAlsoShouldDeletePromotion(string description)
        {
            // Arrange
            var uniqueName = Guid.NewGuid().ToString();

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

            Assert.True(db!.Promotions
                        .Any(x => x.Brand == uniqueName));

            // Act
            var response = await client.DeleteAsync($"/Brands/{uniqueName}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db.Promotions);
        }

        [Theory]
        [InlineData("The Gosho Brand")]
        public async Task RemoveBrand_ShouldBeExecuted_AndAlsoShouldDeleteProductAndPromotion(string description)
        {
            // Arrange
            var uniqueName = Guid.NewGuid().ToString();

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
            await SeedingHelper.SeedProduct(clientHelper,
                "product10",
                            new List<string>
                {
                    "Creatines"
                },
                "301",
                uniqueName,
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");
            await SeedingHelper.SeedPromotion(clientHelper, uniqueName);

            Assert.True(db!.Products
                        .Any(x => x.Brand != null && x.Brand.Name == uniqueName));
            Assert.True(db!.Promotions
                        .Any(x => x.Brand == uniqueName));

            // Act
            var response = await client.DeleteAsync($"/Brands/{uniqueName}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db.Products);
            Assert.Empty(db.Promotions);
        }

        [Fact]
        public async Task RemoveBrand_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.DeleteAsync($"/Brands/NutriBest");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.True(db!.Brands.Any(x => x.Name == "NutriBest"));
        }

        [Fact]
        public async Task RemoveBrand_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.DeleteAsync($"/Brands/NutriBest");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(db!.Brands.Any(x => x.Name == "NutriBest"));
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
