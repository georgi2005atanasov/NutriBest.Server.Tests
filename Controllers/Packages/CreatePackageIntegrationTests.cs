using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Packages
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Infrastructure.Extensions;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Packages.Models;
    using static ErrorMessages.PackagesController;

    [Collection("Packages Controller Tests")]
    public class CreatePackageIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CreatePackageIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task CreatePackage_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var packageModel = new PackageServiceModel
            {
                Grams = 123456,
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(packageModel.Grams.ToString()), "Grams" }
            };

            // Act
            var response = await client.PostAsync("/Packages", formData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(db!.Packages.Any(x => x.Grams == 123456));
        }

        [Fact]
        public async Task CreatePackage_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var packageModel = new PackageServiceModel
            {
                Grams = 1234567
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(packageModel.Grams.ToString()), "Grams" }
            };

            // Act
            var response = await client.PostAsync("/Packages", formData);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(db!.Packages.Any(x => x.Grams == 1234567));
        }

        [Fact]
        public async Task CreatePackage_ShouldReturnBadRequest_WhenPackageExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var packageModel = new PackageServiceModel
            {
                Grams = 1234567
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(packageModel.Grams.ToString()), "Grams" }
            };

            // Act
            await client.PostAsync("/Packages", formData);
            var response = await client.PostAsync("/Packages", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(PackageAlreadyExists, result.Message);
        }

        [Fact]
        public async Task CreatePackage_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var packageModel = new PackageServiceModel
            {
                Grams = 1234567
            };
            
            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(packageModel.Grams.ToString()), "Grams" }
            };
            
            // Act
            var response = await client.PostAsync("/Packages", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreatePackage_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var packageModel = new PackageServiceModel
            {
                Grams = 1234567
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(packageModel.Grams.ToString()), "Grams" }
            };

            // Act
            var response = await client.PostAsync("/Packages", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedPackages();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
