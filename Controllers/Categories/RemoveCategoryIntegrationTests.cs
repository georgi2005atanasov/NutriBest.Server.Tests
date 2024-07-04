namespace NutriBest.Server.Tests.Controllers.Categories
{
    using System.Net;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Categories.Models;
    using Infrastructure.Extensions;

    [Collection("Categories Controller Tests")]
    public class RemoveCategoryIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public RemoveCategoryIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Theory]
        [InlineData("UniqueProducts")]
        public async Task RemoveCategory_ShouldBeExecuted_ForAdmin(string categoryName)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            
            var categoryModel = new CreateCategoryServiceModel
            {
                Name = categoryName
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == categoryName));

            await client.PostAsync("/Categories", formData);

            // Act
            var response = await client.DeleteAsync("/Categories/UniqueProducts");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("true", data);
            Assert.True(!db!.Categories.Any(x => x.Name == "UniqueProducts"));
        }

        [Theory]
        [InlineData("UniqueProducts")]
        public async Task RemoveCategory_ShouldBeExecuted_ForEmployee(string categoryName)
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var categoryModel = new CreateCategoryServiceModel
            {
                Name = categoryName
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == categoryName));

            await client.PostAsync("/Categories", formData);

            // Act
            var response = await client.DeleteAsync("/Categories/UniqueProducts");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("true", data);
            Assert.True(!db!.Categories.Any(x => x.Name == "UniqueProducts"));
        }

        [Theory]
        [InlineData("UniqueProducts")]
        public async Task RemoveCategory_ShouldBeExecuted_AndShouldAlsoDeleteProducts(string categoryName)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var categoryModel = new CreateCategoryServiceModel
            {
                Name = categoryName
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == categoryName));

            await client.PostAsync("/Categories", formData);

            await SeedingHelper.SeedProduct(clientHelper,
                "CategoryProduct",
                new List<string>
                {
                    categoryName
                },
                "100",
                "NutriBest",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "CategoryProduct2",
                new List<string>
                {
                    "Creatines"
                },
                "100",
                "NutriBest",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            Assert.True(db!.ProductsCategories.Any(x => x.Product.Name == "CategoryProduct" &&
                        x.Category.Name == categoryName));
            Assert.True(db!.Products.Any(x => x.ProductsCategories
                                              .Any(x => x.Category.Name == categoryName)));

            // Act
            var response = await client.DeleteAsync("/Categories/UniqueProducts");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("true", data);
            Assert.True(!db!.Categories.Any(x => x.Name == "UniqueProducts"));
            Assert.False(db!.ProductsCategories.Any(x => x.Product.Name == "CategoryProduct" &&
                        x.Category.Name == categoryName));
            Assert.True(!db!.Products.Any(x => x.ProductsCategories
                                              .Any(x => x.Category.Name == categoryName)));
            Assert.Equal(1, db!.Products.Count());
        }

        [Theory]
        [InlineData("UniqueProducts")]
        public async Task RemoveCategory_ShouldBeExecuted_AndShouldAlsoDeletePromotions(string categoryName)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var categoryModel = new CreateCategoryServiceModel
            {
                Name = categoryName
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == categoryName));

            await client.PostAsync("/Categories", formData);

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "NutriBest",
                "TEST PROMO",
                categoryName,
                DateTime.Now,
                "10");

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "NutriBest",
                "TEST PROMO2",
                "Creatines",
                DateTime.Now,
                "10");

            Assert.True(db!.Promotions.Any(x => x.Category == categoryName));

            // Act
            var response = await client.DeleteAsync("/Categories/UniqueProducts");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("true", data);
            Assert.True(!db!.Categories.Any(x => x.Name == "UniqueProducts"));
            Assert.False(db!.Promotions.Any(x => x.Category == categoryName));
            Assert.Equal(1, db!.Promotions.Count());
        }

        [Theory]
        [InlineData("UniqueProducts")]
        public async Task RemoveCategory_ShouldBeExecuted_AndShouldAlsoDeleteProductsAndPromotions(string categoryName)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var categoryModel = new CreateCategoryServiceModel
            {
                Name = categoryName
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == categoryName));

            await client.PostAsync("/Categories", formData);

            await SeedingHelper.SeedProduct(clientHelper,
                "CategoryProduct",
                new List<string>
                {
                    categoryName
                },
                "100",
                "NutriBest",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                "NutriBest",
                "TEST PROMO",
                categoryName,
                DateTime.Now,
                "10");

            Assert.True(db!.ProductsCategories.Any(x => x.Product.Name == "CategoryProduct" &&
                        x.Category.Name == categoryName));
            Assert.True(db!.Products.Any(x => x.ProductsCategories
                                              .Any(x => x.Category.Name == categoryName)));
            Assert.True(db!.Promotions.Any(x => x.Category == categoryName));

            // Act
            var response = await client.DeleteAsync("/Categories/UniqueProducts");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("true", data);
            Assert.True(!db!.Categories.Any(x => x.Name == "UniqueProducts"));
            Assert.False(db!.Promotions.Any(x => x.Category == categoryName));
            Assert.False(db!.ProductsCategories.Any(x => x.Product.Name == "CategoryProduct" &&
                        x.Category.Name == categoryName));
            Assert.True(!db!.Products.Any(x => x.ProductsCategories
                                              .Any(x => x.Category.Name == categoryName)));
        }

        [Theory]
        [InlineData("Creatines")]
        public async Task RemoveCategory_ShouldReturnUnauthorized_ForAnonymous(string categoryName)
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.DeleteAsync($"/Categories/{categoryName}");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(db!.Categories.Any(x => x.Name == categoryName));
        }

        [Theory]
        [InlineData("Creatines")]
        public async Task RemoveCategory_ShouldReturnForbidden_ForUsers(string categoryName)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.DeleteAsync($"/Categories/{categoryName}");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.True(db!.Categories.Any(x => x.Name == categoryName));
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
