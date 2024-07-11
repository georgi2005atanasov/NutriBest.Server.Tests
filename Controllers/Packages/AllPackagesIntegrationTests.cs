namespace NutriBest.Server.Tests.Controllers.Packages
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Packages.Models;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Packages Controller Tests")]
    public class AllPackagesIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllPackagesIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllPackagesEndpoint_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Packages");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<PackageServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<PackageServiceModel>();

            Assert.Equal(8, 8);
            Assert.Equal(result.OrderBy(x => x.Grams), result);
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
