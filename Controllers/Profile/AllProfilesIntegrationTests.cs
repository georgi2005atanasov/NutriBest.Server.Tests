namespace NutriBest.Server.Tests.Controllers.Profile
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Profile.Models;
    using NutriBest.Server.Tests.Controllers.Profile.Data;
    using Infrastructure.Extensions;

    [Collection("Profile Controller Tests")]
    public class AllProfilesIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllProfilesIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllProfilesEndpoint_ShouldBeExecuted_ForAdmin_WithoutFilters()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUsers(clientHelper, 51);

            // Act
            var response = await client.GetAsync("/Profiles?page=1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProfilesServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProfilesServiceModel();

            Assert.Equal(54, result.TotalUsers);
            Assert.Equal(50, result.Profiles.Count);
            Assert.Equal(result.Profiles, result.Profiles
                                            .OrderByDescending(x => x.MadeOn));
        }

        [Fact]
        public async Task AllProfilesEndpoint_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await LargeSeedingHelper.SeedUsers(clientHelper, 51);

            // Act
            var response = await client.GetAsync("/Profiles?page=2");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProfilesServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProfilesServiceModel();

            Assert.Equal(54, result.TotalUsers);
            Assert.Equal(4, result.Profiles.Count);
            Assert.Equal(result.Profiles, result.Profiles
                                            .OrderByDescending(x => x.MadeOn));
        }

        [Theory]
        [MemberData(nameof(ProfileTestData.GetProfileData), MemberType = typeof(ProfileTestData))]
        public async Task AllProfilesEndpoint_ShouldBeExecuted_WithFilters(int page,
            string? search,
            string? groupType,
            int expectedCount)
        {
            // Arrange
            await LargeSeedingHelper.SeedUsers(clientHelper, 20);

            await SeedingHelper.SeedUserOrder(clientHelper, 
                false,
                "user@example.com",
                "user",
                "TEST USER!!!");

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "UNIQUE_USER_3@example.com",
                "3_UNIQUE_USER",
                "Some name");

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "UNIQUE_USER_4@example.com",
                "4_UNIQUE_USER",
                "Some name");

            var client = await clientHelper.GetAdministratorClientAsync();

            var query = $"?page={page}&search={search}&groupType={groupType}";

            // Act
            var response = await client.GetAsync($"/Profiles{query}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllProfilesServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllProfilesServiceModel();

            Assert.Equal(expectedCount, result.Profiles.Count);
        }

        [Fact]
        public async Task AllProfilesEndpoint_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.GetAsync("/Profiles?page=1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AllProfilesEndpoint_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.GetAsync("/Profiles?page=1");

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
            db.SeedBgCities();
            db.SeedDeCities();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
