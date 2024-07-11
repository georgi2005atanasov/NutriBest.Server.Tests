using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Products
{
    using System.Net;
    using System.Text.Json;
    using Moq;
    using Xunit;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Products.Models;
    using NutriBest.Server.Infrastructure.Extensions;
    using static ErrorMessages;

    [Collection("Products Controller Tests")]
    public class CreateProductIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CreateProductIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task CreateProduct_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var productModel = new CreateProductRequestModel
            {
                Name = "product400",
                Categories = new List<string>
                {
                    "Creatines"
                },
                Price = "100",
                Brand = "Klean Athlete",
                ProductSpecs = "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]",
                Description = "Some random descr",
                Image = new Mock<IFormFile>().Object
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(productModel.Description), "Description" },
                { new StringContent(productModel.Name), "Name" },
                { new StringContent(productModel.Brand), "Brand" },
                { new StringContent(productModel.Price), "Price" },
                { new StringContent(productModel.ProductSpecs), "ProductSpecs" }
            };

            if (productModel.Image != null)
            {
                using var ms = new MemoryStream();
                await productModel.Image.CopyToAsync(ms);
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                formData.Add(fileContent, "Image", "FakeName");
            }

            for (int i = 0; i < productModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(productModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            Assert.False(db!.Products.Any(x => x.Name == "product400"));
            var response = await client.PostAsync("/Products", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(db!.Products.Any(x => x.Name == "product400"));
        }

        [Fact]
        public async Task CreateProduct_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var productModel = new CreateProductRequestModel
            {
                Name = "product401",
                Categories = new List<string>
                {
                    "Creatines"
                },
                Price = "100",
                Brand = "Klean Athlete",
                ProductSpecs = "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]",
                Description = "Some random descr",
                Image = new Mock<IFormFile>().Object
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(productModel.Description), "Description" },
                { new StringContent(productModel.Name), "Name" },
                { new StringContent(productModel.Brand), "Brand" },
                { new StringContent(productModel.Price), "Price" },
                { new StringContent(productModel.ProductSpecs), "ProductSpecs" }
            };

            if (productModel.Image != null)
            {
                using var ms = new MemoryStream();
                await productModel.Image.CopyToAsync(ms);
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                formData.Add(fileContent, "Image", "FakeName");
            }

            for (int i = 0; i < productModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(productModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            Assert.False(db!.Products.Any(x => x.Name == "product401"));
            var response = await client.PostAsync("/Products", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.True(db!.Products.Any(x => x.Name == "product401"));
        }

        [Fact]
        public async Task CreateProduct_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var productModel = new CreateProductRequestModel
            {
                Name = "product403",
                Categories = new List<string>
                {
                    "Creatines"
                },
                Price = "100",
                Brand = "Klean Athlete",
                ProductSpecs = "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]",
                Description = "Some random descr",
                Image = new Mock<IFormFile>().Object
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(productModel.Description), "Description" },
                { new StringContent(productModel.Name), "Name" },
                { new StringContent(productModel.Brand), "Brand" },
                { new StringContent(productModel.Price), "Price" },
                { new StringContent(productModel.ProductSpecs), "ProductSpecs" }
            };

            if (productModel.Image != null)
            {
                using var ms = new MemoryStream();
                await productModel.Image.CopyToAsync(ms);
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                formData.Add(fileContent, "Image", "FakeName");
            }

            for (int i = 0; i < productModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(productModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            Assert.False(db!.Products.Any(x => x.Name == "product403"));
            var response = await client.PostAsync("/Products", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(db!.Products.Any(x => x.Name == "product403"));
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
