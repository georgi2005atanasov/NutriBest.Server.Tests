namespace NutriBest.Server.Tests.Controllers.Flavours
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Infrastructure.Extensions;
    using NutriBest.Server.Features.Flavours.Models;
    using System.Text.Json;

    [Collection("Flavours Controller Tests")]
    public class AllFlavoursIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllFlavoursIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllFlavoursEndpoint_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Flavours");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<FlavourServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<FlavourServiceModel>();

            Assert.Equal(15, result.Count());
            Assert.Equal(result.OrderBy(x => x.Name), result);
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
