using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Flavours
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Infrastructure.Extensions;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Flavours.Models;
    using static ErrorMessages.FlavoursController;

    [Collection("Flavours Controller Tests")]
    public class CreateFlavourIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CreateFlavourIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task CreateFlavour_ShouldBeExecuted_ForAdmin()
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

            // Act
            var response = await client.PostAsync("/Flavours", formData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(db!.Flavours.Any(x => x.FlavourName == flavourName));
        }

        [Fact]
        public async Task CreateFlavour_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var flavourName = Guid.NewGuid().ToString();
            var client = await clientHelper.GetEmployeeClientAsync();
            var flavourModel = new FlavourServiceModel
            {
                Name = flavourName,
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(flavourModel.Name), "Name" }
            };

            // Act
            var response = await client.PostAsync("/Flavours", formData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(db!.Flavours.Any(x => x.FlavourName == flavourName));
        }

        [Fact]
        public async Task CreateFlavour_ShouldReturnBadRequest_WhenFlavourExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var flavourModel = new FlavourServiceModel
            {
                Name = "Chocolate", // ENSURE IT EXISTS!!!
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(flavourModel.Name), "Name" }
            };

            // Act
            await client.PostAsync("/Flavours", formData);
            var response = await client.PostAsync("/Flavours", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(FlavourAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreateFlavour_ShouldReturnUnathorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var flavourModel = new FlavourServiceModel
            {
                Name = Guid.NewGuid().ToString(),
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(flavourModel.Name), "Name" }
            };

            // Act
            var response = await client.PostAsync("/Flavours", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateFlavour_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            var flavourModel = new FlavourServiceModel
            {
                Name = Guid.NewGuid().ToString(),
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(flavourModel.Name), "Name" }
            };

            // Act
            var response = await client.PostAsync("/Flavours", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedFlavours();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
