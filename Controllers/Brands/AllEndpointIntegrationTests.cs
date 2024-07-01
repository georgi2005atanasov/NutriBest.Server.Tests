namespace NutriBest.Server.Tests.Controllers.Brands
{
    using Xunit;
    using System.Text.Json; 
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Brands.Models;
    using Infrastructure.Extensions;

    [Collection("Brands Controller Tests")]
    public class AllEndpointIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllEndpointIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllEndpoint_ShouldReturnAllBrands()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Brands");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<IEnumerable<BrandServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<BrandServiceModel>();

            Assert.Equal(6, result.Count());
            Assert.Equal(result.OrderBy(x => x.Name), result);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedDatabase(scope);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
