namespace NutriBest.Server.Tests.Controllers.Brands
{
    using Xunit;
    using System.Text.Json; 
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Brands.Models;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Brands Controller Tests")]
    public class AllBrandsIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllBrandsIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllBrandsEndpoint_ShouldReturnAllBrands()
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

            Assert.Equal(7, result.Count());
            Assert.Equal(result.OrderBy(x => x.Name), result);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedBrands();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
