namespace NutriBest.Server.Tests.Controllers.Categories
{
    using Xunit;
    using System.Text.Json;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Categories.Models;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Categories Controller Tests")]
    public class AllCategoriesIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllCategoriesIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllCategoriesEndpoint_ShouldBeExecuted()
        {
            var client = clientHelper.GetAnonymousClient();

            var response = await client.GetAsync("/Categories");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<IEnumerable<CategoryServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CategoryServiceModel>();

            Assert.Equal(12, result.Count());
            Assert.Equal(result.OrderBy(x => x.Name), result);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedCategories();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
