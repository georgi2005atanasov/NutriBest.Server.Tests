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
    using static ErrorMessages.ProductsController;
    using static ErrorMessages.BrandsController;
    using static ErrorMessages.PackagesController;
    using static ErrorMessages.FlavoursController;

    [Collection("Products Controller Tests")]
    public class UpdateProductIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public UpdateProductIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task UpdateProduct_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "product71",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            var updateModel = new UpdateProductServiceModel
            {
                Name = "Updated Product Name",
                Brand = "Muscle Tech",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                // I HAVE PUT COMMA FOR PRICE SEPARATOR
                ProductSpecs = "[{ \"flavour\": \"Chocolate\", \"grams\": 2000, \"quantity\": 100, \"price\": \"15,99\"}]",
                Image = new Mock<IFormFile>().Object
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" }
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");

            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("1", data);

            var updatedProduct = db!.Products
                .FirstOrDefault(x => x.Name == "Updated Product Name");
            Assert.NotNull(updatedProduct);
            Assert.Equal("Updated Product Name", updatedProduct!.Name);
            Assert.Equal("Some new updated description", updatedProduct!.Description);
            Assert.Equal("Muscle Tech", updatedProduct!.Brand!.Name);
            Assert.Contains("Recovery", db.ProductsCategories
                                        .Select(x => x.Category.Name));
            Assert.True(db.ProductsPackagesFlavours
                .Where(x => x.Flavour!.FlavourName == "Chocolate" &&
                       x.Package!.Grams == 2000 &&
                       x.ProductId == 1).Count() == 1);
            Assert.Single(db.ProductsCategories);
        }

        [Fact]
        public async Task UpdateProduct_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "product71",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            var updateModel = new UpdateProductServiceModel
            {
                Name = "Updated Product Name",
                Brand = "Muscle Tech",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                // I HAVE PUT COMMA FOR PRICE SEPARATOR
                ProductSpecs = "[{ \"flavour\": \"Chocolate\", \"grams\": 2000, \"quantity\": 100, \"price\": \"15,99\"}]",
                Image = new Mock<IFormFile>().Object
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" }
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");


            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("1", data);

            var updatedProduct = db!.Products
                .FirstOrDefault(x => x.Name == "Updated Product Name");
            Assert.NotNull(updatedProduct);
            Assert.Equal("Updated Product Name", updatedProduct!.Name);
            Assert.Equal("Some new updated description", updatedProduct!.Description);
            Assert.Equal("Muscle Tech", updatedProduct!.Brand!.Name);
            Assert.Contains("Recovery", db.ProductsCategories
                                        .Select(x => x.Category.Name));
            Assert.True(db.ProductsPackagesFlavours
                .Where(x => x.Flavour!.FlavourName == "Chocolate" &&
                       x.Package!.Grams == 2000 &&
                       x.ProductId == 1).Count() == 1);
            Assert.Single(db.ProductsCategories);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenNoProductSpecsArePassed()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "product71",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            var updateModel = new UpdateProductServiceModel
            {
                Name = "Updated Product Name",
                Brand = "Muscle Tech",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                Image = new Mock<IFormFile>().Object,
                ProductSpecs = "[]"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" },
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");

            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("ProductSpecs", result.Key);
            Assert.Equal(ProductsSpecificationAreRequired, result.Message);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("4001")]
        [InlineData("0")]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenProductSpecPriceIsInvalidNumber(
            string price)
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "product71",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            var invalidSpecs = "[{\"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"{0}\"}]";
            invalidSpecs = invalidSpecs.Replace("{0}", price);

            var updateModel = new UpdateProductServiceModel
            {
                Name = "Updated Product Name",
                Brand = "Muscle Tech",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                Image = new Mock<IFormFile>().Object,
                ProductSpecs = invalidSpecs
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" },
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");

            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("ProductSpecs", result.Key);
            Assert.Equal(InvalidPriceRange, result.Message);
        }

        [Theory]
        [InlineData("pesho")]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenProductSpecPriceIsNaN(
            string price)
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "product71",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            var invalidSpecs = "[{\"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"{0}\"}]";
            invalidSpecs = invalidSpecs.Replace("{0}", price);

            var updateModel = new UpdateProductServiceModel
            {
                Name = "Updated Product Name",
                Brand = "Muscle Tech",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                Image = new Mock<IFormFile>().Object,
                ProductSpecs = invalidSpecs
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" },
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");

            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Price", result.Key);
            Assert.Equal(PricesMustBeNumbers, result.Message);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenProductDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var invalidSpecs = "[{\"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"10\"}]";
         
            var updateModel = new UpdateProductServiceModel
            {
                Name = "Updated Product Name",
                Brand = "Muscle Tech",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                Image = new Mock<IFormFile>().Object,
                ProductSpecs = invalidSpecs
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" },
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");

            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/231", formData);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenProductWithThisNameExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var invalidSpecs = "[{\"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"10\"}]";

            var updateModel = new UpdateProductServiceModel
            {
                Name = "product72",
                Brand = "Muscle Tech",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                Image = new Mock<IFormFile>().Object,
                ProductSpecs = invalidSpecs
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" },
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");

            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(ProductAlreadyExists, result.Message);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenBrandDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var invalidSpecs = "[{\"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"10\"}]";

            var updateModel = new UpdateProductServiceModel
            {
                Name = "UNIQUE69",
                Brand = "InvalidBrand69",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                Image = new Mock<IFormFile>().Object,
                ProductSpecs = invalidSpecs
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" },
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");

            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidBrandName, result.Message);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenPackageDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var invalidSpecs = "[{\"flavour\": \"Coconut\", \"grams\": 123532, \"quantity\": 100, \"price\": \"10\"}]";

            var updateModel = new UpdateProductServiceModel
            {
                Name = "UNIQUE69",
                Brand = "NutriBest",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                Image = new Mock<IFormFile>().Object,
                ProductSpecs = invalidSpecs
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" },
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");

            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidPackage, result.Message);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnBadRequest_WhenFlavourDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            var invalidSpecs = "[{\"flavour\": \"NotExistingFlavour\", \"grams\": 1000, \"quantity\": 100, \"price\": \"10\"}]";

            var updateModel = new UpdateProductServiceModel
            {
                Name = "UNIQUE69",
                Brand = "NutriBest",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                Image = new Mock<IFormFile>().Object,
                ProductSpecs = invalidSpecs
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" },
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");

            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidFlavour, result.Message);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedProduct(clientHelper,
                "product71",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            var updateModel = new UpdateProductServiceModel
            {
                Name = "Updated Product Name",
                Brand = "Muscle Tech",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                // I HAVE PUT COMMA FOR PRICE SEPARATOR
                ProductSpecs = "[{ \"flavour\": \"Chocolate\", \"grams\": 2000, \"quantity\": 100, \"price\": \"15,99\"}]",
                Image = new Mock<IFormFile>().Object
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" }
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");


            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var updatedProduct = db!.Products
                .FirstOrDefault(x => x.Name == "Updated Product Name");
            Assert.Null(updatedProduct);
        }

        [Fact]
        public async Task UpdateProduct_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "product71",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            var updateModel = new UpdateProductServiceModel
            {
                Name = "Updated Product Name",
                Brand = "Muscle Tech",
                Description = "Some new updated description",
                Categories = new List<string>
                {
                    "Recovery"
                },
                // I HAVE PUT COMMA FOR PRICE SEPARATOR
                ProductSpecs = "[{ \"flavour\": \"Chocolate\", \"grams\": 2000, \"quantity\": 100, \"price\": \"15,99\"}]",
                Image = new Mock<IFormFile>().Object
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.Name), "Name" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.ProductSpecs), "ProductSpecs" }
            };

            using var ms = new MemoryStream();
            await updateModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName");


            for (int i = 0; i < updateModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(updateModel.Categories[i]), $"Categories[{i}]");
            }

            // Act
            var response = await client.PutAsync("/Products/1", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var updatedProduct = db!.Products
                .FirstOrDefault(x => x.Name == "Updated Product Name");
            Assert.Null(updatedProduct);
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
