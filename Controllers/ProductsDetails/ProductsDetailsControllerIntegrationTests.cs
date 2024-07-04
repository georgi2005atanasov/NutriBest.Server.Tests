using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.ProductsDetails
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.ProductsDetails.Models;
    using Infrastructure.Extensions;
    using static ErrorMessages.ProductsController;

    [Collection("Products Details Controller Tests")]
    public class ProductsDetailsControllerIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public ProductsDetailsControllerIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetDetailsById_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            await SeedingHelper.SeedProduct(clientHelper,
                "product80",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            // Act
            var response = await client.GetAsync("/Products/Details/1/product80");
            var data = await response.Content.ReadAsStringAsync();
            
            // Assert
            var result = JsonSerializer.Deserialize<ProductDetailsServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ProductDetailsServiceModel();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("product80", result.Name);
            Assert.Equal(15.99m, result.Price);
            Assert.Equal("this is product 1", result.Description);
            Assert.Equal("Klean Athlete", result.Brand);
            Assert.Contains("Creatines", result.Categories);
        }

        [Theory]
        [InlineData(1, "invalidName")]
        [InlineData(900, "product80")]
        public async Task GetDetailsById_ShouldReturnBadRequest_ForInvalidDataPassed(int id,
            string name)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            await SeedingHelper.SeedProduct(clientHelper,
                "product80",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            // Act
            var response = await client.GetAsync($"/Products/Details/{id}/{name}");
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
        public async Task SetDetailsShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var detailsModel = new CreateProductDetailsServiceModel
            {
                HowToUse = "Just like that",
                Ingredients = "Cocaine, Meth, Extasy",
                ServingSize = "Till collapse",
                WhyChoose = "Don't choose it"
            };
            var formData = new MultipartFormDataContent
            {
                { new StringContent(detailsModel.HowToUse), "HowToUse" },
                { new StringContent(detailsModel.Ingredients), "Ingredients" },
                { new StringContent(detailsModel.ServingSize), "ServingSize" },
                { new StringContent(detailsModel.WhyChoose), "WhyChoose" },
            };

            await SeedingHelper.SeedProduct(clientHelper,
                "product80",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            // Act
            var response = await client.PostAsync("/Products/Details/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);
            Assert.True(db!.ProductsDetails.Any(x => x.ProductId == 1 &&
                x.Ingredients == "Cocaine, Meth, Extasy" &&
                x.ServingSize == "Till collapse" &&
                x.HowToUse == "Just like that" &&
                x.WhyChoose == "Don't choose it"));
        }

        public async Task SetDetailsShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var detailsModel = new CreateProductDetailsServiceModel
            {
                HowToUse = "Just like that",
                Ingredients = "Cocaine, Meth, Extasy",
                ServingSize = "Till collapse",
                WhyChoose = "Don't choose it"
            };
            var formData = new MultipartFormDataContent
            {
                { new StringContent(detailsModel.HowToUse), "HowToUse" },
                { new StringContent(detailsModel.Ingredients), "Ingredients" },
                { new StringContent(detailsModel.ServingSize), "ServingSize" },
                { new StringContent(detailsModel.WhyChoose), "WhyChoose" },
            };

            await SeedingHelper.SeedProduct(clientHelper,
                "product80",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            // Act
            var response = await client.PostAsync("/Products/Details/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);
            Assert.True(db!.ProductsDetails.Any(x => x.ProductId == 1 &&
                x.Ingredients == "Cocaine, Meth, Extasy" &&
                x.ServingSize == "Till collapse" &&
                x.HowToUse == "Just like that" &&
                x.WhyChoose == "Don't choose it"));
        }

        [Theory]
        [InlineData(101)]
        public async Task SetDetailsShouldReturnBadRequest_WithInvalidIdPassed(int id)
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var detailsModel = new CreateProductDetailsServiceModel
            {
                HowToUse = "Just like that",
                Ingredients = "Cocaine, Meth, Extasy",
                ServingSize = "Till collapse",
                WhyChoose = "Don't choose it"
            };
            var formData = new MultipartFormDataContent
            {
                { new StringContent(detailsModel.HowToUse), "HowToUse" },
                { new StringContent(detailsModel.Ingredients), "Ingredients" },
                { new StringContent(detailsModel.ServingSize), "ServingSize" },
                { new StringContent(detailsModel.WhyChoose), "WhyChoose" },
            };

            await SeedingHelper.SeedProduct(clientHelper,
                "product80",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            // Act
            var response = await client.PostAsync($"/Products/Details/{id}", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("false", data);
        }


        [Fact]
        public async Task SetDetailsShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var detailsModel = new CreateProductDetailsServiceModel
            {
                HowToUse = "Just like that",
                Ingredients = "Cocaine, Meth, Extasy",
                ServingSize = "Till collapse",
                WhyChoose = "Don't choose it"
            };
            var formData = new MultipartFormDataContent
            {
                { new StringContent(detailsModel.HowToUse), "HowToUse" },
                { new StringContent(detailsModel.Ingredients), "Ingredients" },
                { new StringContent(detailsModel.ServingSize), "ServingSize" },
                { new StringContent(detailsModel.WhyChoose), "WhyChoose" },
            };

            await SeedingHelper.SeedProduct(clientHelper,
                "product80",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            // Act
            var response = await client.PostAsync($"/Products/Details/1", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(db!.ProductsDetails.Any(x => x.ProductId == 1 &&
                string.IsNullOrEmpty(x.Ingredients) &&
                string.IsNullOrEmpty(x.ServingSize)&&
                string.IsNullOrEmpty(x.HowToUse)&&
                string.IsNullOrEmpty(x.WhyChoose)));
        }

        [Fact]
        public async Task SetDetailsShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            var detailsModel = new CreateProductDetailsServiceModel
            {
                HowToUse = "Just like that",
                Ingredients = "Cocaine, Meth, Extasy",
                ServingSize = "Till collapse",
                WhyChoose = "Don't choose it"
            };
            var formData = new MultipartFormDataContent
            {
                { new StringContent(detailsModel.HowToUse), "HowToUse" },
                { new StringContent(detailsModel.Ingredients), "Ingredients" },
                { new StringContent(detailsModel.ServingSize), "ServingSize" },
                { new StringContent(detailsModel.WhyChoose), "WhyChoose" },
            };

            await SeedingHelper.SeedProduct(clientHelper,
                "product80",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            // Act
            var response = await client.PostAsync($"/Products/Details/1", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.True(db!.ProductsDetails.Any(x => x.ProductId == 1 &&
                string.IsNullOrEmpty(x.Ingredients) &&
                string.IsNullOrEmpty(x.ServingSize) &&
                string.IsNullOrEmpty(x.HowToUse) &&
                string.IsNullOrEmpty(x.WhyChoose)));
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
