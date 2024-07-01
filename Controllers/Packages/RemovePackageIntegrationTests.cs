using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Packages
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using Infrastructure.Extensions;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Packages.Models;
    using static ErrorMessages.PackagesController;
    using Microsoft.EntityFrameworkCore;

    [Collection("Packages Controller Tests")]
    public class RemovePackageIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public RemovePackageIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task RemovePackage_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            int grams = 12345678;
            var client = await clientHelper.GetAdministratorClientAsync();

            var packageModel = new PackageServiceModel
            {
                Grams = grams
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(grams.ToString()), "Grams" }
            };

            await client.PostAsync("/Packages", formData);

            // Act
            var response = await client.DeleteAsync($"/Packages/{grams}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(!db!.Packages.Any(x => x.Grams == grams));
        }

        [Fact]
        public async Task RemovePackage_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            int grams = 12345678;
            var client = await clientHelper.GetEmployeeClientAsync();

            var packageModel = new PackageServiceModel
            {
                Grams = grams
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(grams.ToString()), "Grams" }
            };

            await client.PostAsync("/Packages", formData);

            // Act
            var response = await client.DeleteAsync($"/Packages/{grams}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(!db!.Packages.Any(x => x.Grams == grams));
        }

        [Fact]
        public async Task RemovePackage_ShouldBeExecuted_AndAlsoShouldRemoveProducts()
        {
            // Arrange
            int grams = 500; // ENSURE IT EXISTS
            var client = await clientHelper.GetEmployeeClientAsync();

            var packageModel = new PackageServiceModel
            {
                Grams = grams
            };

            await SeedingHelper.SeedProduct(clientHelper, 
                "removePackageProducts", 
                "Klean Athlete"); // ENSURE "Klean Athlete" EXISTS!!!

            Assert.True(db!.ProductsPackagesFlavours
                .Any(x => x.Package!.Grams == grams));

            // Act
            var response = await client.DeleteAsync($"/Packages/{grams}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(!db!.Packages.Any(x => x.Grams == grams));
            Assert.True(!db!.ProductsPackagesFlavours
                .Any(x => x.Package!.Grams == grams));
        }

        [Fact]
        public async Task RemovePackage_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.DeleteAsync($"/Packages/99932");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task RemovePackage_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.DeleteAsync($"/Packages/99932");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
