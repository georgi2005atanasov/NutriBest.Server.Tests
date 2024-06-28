namespace NutriBest.Server.Tests.Controllers.Cities
{
    using Xunit;
    using System.Text.Json;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Cities.Models;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Home Controller Tests")]
    public class CitiesControllerIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CitiesControllerIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllCitiesWithCountries_ShouldReturn_AllCitiesWithCountries()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.GetAsync("/Cities");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<List<AllCitiesWithCountryServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<AllCitiesWithCountryServiceModel>();

            Assert.Contains("Germany", result.Select(x => x.Country));
            Assert.Contains("Bulgaria", result.Select(x => x.Country));
            Assert.Equal(129, result
                                .Where(x => x.Country == "Germany")
                                .Select(x => x.Cities)
                                .SelectMany(x => x!)
                                .Count());
            Assert.Equal(256, result
                                .Where(x => x.Country == "Bulgaria")
                                .Select(x => x.Cities)
                                .SelectMany(x => x!)
                                .Count());
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
