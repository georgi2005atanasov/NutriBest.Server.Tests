using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.NutritionFacts
{
    using System.Net;
    using System.Text.Json;
    using System.Net.Http.Json;
    using System.Collections.Generic;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.NutritionsFacts.Models;
    using NutriBest.Server.Infrastructure.Extensions;
    using static ErrorMessages.NutritionFactsController;

    [Collection("Nutrition Facts Controller Tests")]
    public class SetNutriFactsIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public SetNutriFactsIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task SetNutriFacts_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "NutriFactsProduct",
                new List<string>
                {
                    "Creatines"
                },
                "100",
                "NutriBest",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            var nutriFactsToSeed = new NutritionFactsServiceModel
            {
                Carbohydrates = "10",
                EnergyValue = "100",
                Fats = "5",
                Proteins = "15",
                Salt = "0.1",
                SaturatedFats = "30",
                Sugars = "0"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Products/NutriFacts/1", nutriFactsToSeed);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("true", data);
        }

        [Fact]
        public async Task SetNutriFacts_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "NutriFactsProduct",
                new List<string>
                {
                    "Creatines"
                },
                "100",
                "NutriBest",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            var nutriFactsToSeed = new NutritionFactsServiceModel
            {
                Carbohydrates = "10",
                EnergyValue = "100",
                Fats = "5",
                Proteins = "15",
                Salt = "0.1",
                SaturatedFats = "30",
                Sugars = "0"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Products/NutriFacts/1", nutriFactsToSeed);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal("true", data);
        }

        [Theory]
        [InlineData("10", "10", "10", "pesho", "misho", "10", "10")]
        [InlineData("10", "10", "10", "0", "misho", "10", "10")]
        [InlineData("pesho", "gosho", "misho", "0", "misho", "10", "10")]
        public async Task SetNutriFacts_ShouldReturnBadRequest_ForInvalidNumbers(string carbohydrates,
            string energyValue,
            string fats,
            string proteins,
            string salt,
            string saturatedFats,
            string sugars)
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "NutriFactsProduct",
                new List<string>
                {
                    "Creatines"
                },
                "100",
                "NutriBest",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            var nutriFactsToSeed = new NutritionFactsServiceModel
            {
                Carbohydrates = carbohydrates,
                EnergyValue = energyValue,
                Fats = fats,
                Proteins = proteins,
                Salt = salt,
                SaturatedFats = saturatedFats,
                Sugars = sugars
            };

            // Act
            var response = await client.PostAsJsonAsync("/Products/NutriFacts/1", nutriFactsToSeed);
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            // Assert
            Assert.Equal(InvalidNutritionFacts, result.Message);
        }


        [Fact]
        public async Task SetNutriFacts_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedProduct(clientHelper,
                "NutriFactsProduct",
                new List<string>
                {
                    "Creatines"
                },
                "100",
                "NutriBest",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            var nutriFactsToSeed = new NutritionFactsServiceModel
            {
                Carbohydrates = "10",
                EnergyValue = "100",
                Fats = "5",
                Proteins = "15",
                Salt = "0.1",
                SaturatedFats = "30",
                Sugars = "0"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Products/NutriFacts/1", nutriFactsToSeed);
            
            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SetNutriFacts_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "NutriFactsProduct",
                new List<string>
                {
                    "Creatines"
                },
                "100",
                "NutriBest",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            var nutriFactsToSeed = new NutritionFactsServiceModel
            {
                Carbohydrates = "10",
                EnergyValue = "100",
                Fats = "5",
                Proteins = "15",
                Salt = "0.1",
                SaturatedFats = "30",
                Sugars = "0"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Products/NutriFacts/1", nutriFactsToSeed);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedPackages();
            db.SeedCategories();
            db.SeedFlavours();
            db.SeedBrands();
        }
        
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
