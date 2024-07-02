using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Flavours
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Extensions;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Flavours.Models;
    using static ErrorMessages.FlavoursController;

    [Collection("Flavours Controller Tests")]
    public class RemoveFlavourIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public RemoveFlavourIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task RemoveFlavour_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var flavourName = Guid.NewGuid().ToString();
            var client = await clientHelper.GetAdministratorClientAsync();

            var flavourModel = new FlavourServiceModel
            {
                Name = flavourName
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(flavourModel.Name), "Name" }
            };

            await client.PostAsync("/Flavours", formData);

            // Act
            var response = await client.DeleteAsync($"/Flavours/{flavourName}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(!db!.Flavours.Any(x => x.FlavourName == flavourName));
        }

        [Fact]
        public async Task RemoveFlavour_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var flavourName = Guid.NewGuid().ToString();
            var client = await clientHelper.GetEmployeeClientAsync();

            var flavourModel = new FlavourServiceModel
            {
                Name = flavourName
            };
            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(flavourModel.Name), "Name" }
            };

            await client.PostAsync("/Flavours", formData);

            // Act
            var response = await client.DeleteAsync($"/Flavours/{flavourName}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(!db!.Flavours.Any(x => x.FlavourName == flavourName));
        }

        [Fact]
        public async Task RemoveFlavour_ShouldBeExecuted_AndShouldAlsoRemoveProduct()
        {
            // Arrange
            var flavourName = "Coconut"; // ENSURE IT EXISTS
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedProduct(clientHelper,
                "product41",
                            new List<string>
                {
                    "Creatines"
                },
                "100",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 500, \"quantity\": 100, \"price\": \"99.99\"}]");

            Assert.True(db!.ProductsPackagesFlavours
                        .Any(x => x.Flavour!.FlavourName == "Coconut"));

            var flavourModel = new FlavourServiceModel
            {
                Name = flavourName
            };
            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(flavourModel.Name), "Name" }
            };

            await client.PostAsync("/Flavours", formData);

            // Act
            var response = await client.DeleteAsync($"/Flavours/{flavourName}");

            // Assert
            Assert.Empty(db.ProductsPackagesFlavours);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RemoveBrand_ShouldReturnBadRequest_WhenFlavourDoesNotExists()
        {
            // Arrange
            var flavourName = Guid.NewGuid().ToString();
            var client = await clientHelper.GetAdministratorClientAsync();

            var flavourModel = new FlavourServiceModel
            {
                Name = flavourName
            };
            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(flavourModel.Name), "Name" }
            };

            await client.PostAsync("/Flavours", formData);

            // Act
            await client.DeleteAsync($"/Flavours/{flavourName}");
            var response = await client.DeleteAsync($"/Flavours/{flavourName}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidFlavour, result.Message);
        }

        [Fact]
        public async Task RemoveBrand_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.DeleteAsync($"/Flavours/Coconut");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.True(db!.Flavours.Any(x => x.FlavourName == "Coconut"));
        }

        [Fact]
        public async Task RemoveBrand_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.DeleteAsync($"/Flavours/Coconut");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(db!.Flavours.Any(x => x.FlavourName == "Coconut"));
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
