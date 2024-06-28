namespace NutriBest.Server.Tests.Controllers.Admin
{
    using Xunit;
    using System.Net;
    using System.Text.Json;
    using NutriBest.Server.Features.Admin.Models;

    [Collection("Admin Controller Tests")]
    public class AdminControllerIntegrationTests : IAsyncLifetime
    {
        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        public AdminControllerIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllUsersEndpoint_ShouldReturnAllUsers()
        {
            //Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var allUsers = await client.GetAsync("/Admin/AllUsers");
            var data = await allUsers.Content.ReadAsStringAsync();

            // Assert
            var allUsersModel = JsonSerializer.Deserialize<IEnumerable<UserServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new List<UserServiceModel>();

            Assert.Equal(3, allUsersModel.Count());
        }

        [Fact]
        public async Task AllUsersEndpoint_ShouldReturnUnauthorizedForEmployees()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            // Act
            var allUsers = await client.GetAsync("/Admin/AllUsers");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, allUsers.StatusCode);
        }

        [Fact]
        public async Task AllUsersEndpoint_ShouldReturnUnauthorizedForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var allUsers = await client.GetAsync("/Admin/AllUsers");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, allUsers.StatusCode);
        }

        [Fact]
        public async Task AllUsersEndpoint_ShouldReturnUnauthorizedForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var allUsers = await client.GetAsync("/Admin/AllUsers");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, allUsers.StatusCode);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
