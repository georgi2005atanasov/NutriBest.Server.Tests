namespace NutriBest.Server.Tests.Controllers.Newsletter
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using System.Net;
    using System.Text.Json;
    using NutriBest.Server.Infrastructure.Extensions;
    using NutriBest.Server.Features.Newsletter.Models;
    using NutriBest.Server.Tests.Controllers.Newsletter.Data;

    [Collection("Newsletter Controller Tests")]
    public class AllSubscribersIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllSubscribersIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllSubscribersEndpoint_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUsersAsSubscribers(clientHelper, false, 10);
            await LargeSeedingHelper.SeedUsersAsSubscribers(clientHelper, true, 11);

            // Act
            var response = await client.GetAsync("/Newsletter?page=1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllSubscribersServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllSubscribersServiceModel();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(21, result.TotalSubscribers);
            Assert.Equal(result.Subscribers, result.Subscribers
                         .OrderByDescending(x => x.RegisteredOn));
        }

        [Fact]
        public async Task AllSubscribersEndpoint_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await LargeSeedingHelper.SeedUsersAsSubscribers(clientHelper, false, 10);

            // Act
            var response = await client.GetAsync("/Newsletter?page=1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllSubscribersServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllSubscribersServiceModel();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(10, result.TotalSubscribers);
            Assert.Equal(result.Subscribers, result.Subscribers
                         .OrderByDescending(x => x.RegisteredOn));
        }

        [Fact]
        public async Task AllSubscribersEndpoint_ShouldReturnOneSubscriber_OnSecondPage()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUsersAsSubscribers(clientHelper, false, 50);
            await LargeSeedingHelper.SeedUsersAsSubscribers(clientHelper, true, 11);

            // Act
            var response = await client.GetAsync("/Newsletter?page=2");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllSubscribersServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllSubscribersServiceModel();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(61, result.TotalSubscribers);
            Assert.Equal(11, result.Subscribers.Count);
        }

        [Theory]
        [MemberData(nameof(SubscriberTestData.GetSubscriberData), MemberType = typeof(SubscriberTestData))]
        public async Task AllSubscribersEndpoint_ShouldBeExecuted_WithFilters(int page,
            string? search,
            string? groupType,
            int expectedCount)
        {
            // Arrange
            var admin = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUsersAsSubscribers(clientHelper, false, 10);

            // seed also 'user' in order to have somebody with orders
            var anonymous = clientHelper.GetAnonymousClient();
            var formData = new MultipartFormDataContent
            {
                { new StringContent("user@example.com"), "email" },
            };

            await anonymous.PostAsync("/Newsletter", formData);

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

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "UNIQUE_USER_5@example.com",
                "5_UNIQUE_USER",
                "Some name");
            
            var query = $"?page={page}&search={search}&groupType={groupType}";

            // Act
            var response = await admin.GetAsync($"/Newsletter{query}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllSubscribersServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllSubscribersServiceModel();

            Assert.Equal(expectedCount, result.Subscribers.Count);
        }

        [Fact]
        public async Task AllSubscribersEndpoint_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await LargeSeedingHelper.SeedUsersAsSubscribers(clientHelper, false, 10);

            // Act
            var response = await client.GetAsync("/Newsletter?page=1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("", data);
        }

        [Fact]
        public async Task AllSubscribersEndpoint_ShouldReturnForbidden_ForAnonymous()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await LargeSeedingHelper.SeedUsersAsSubscribers(clientHelper, false, 10);

            // Act
            var response = await client.GetAsync("/Newsletter?page=1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("", data);
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
