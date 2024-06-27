namespace NutriBest.Server.Tests.Controllers.Identity
{
    using Xunit;
    using System.Net;
    using System.Text.Json;
    using NutriBest.Server.Features.Identity.Models;

    [Collection("Identity Controller Tests")]
    public class RolesIntegrationTests : IAsyncLifetime
    {
        private ClientHelper clientHelper;
        private CustomWebApplicationFactoryFixture fixture;

        public RolesIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task RolesEndpoint_ShouldReturnRolesServiceModel()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Identity/Roles");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var rolesResult = JsonSerializer
                .Deserialize<RolesServiceModel>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new RolesServiceModel();

            Assert.Equal(2, rolesResult.Roles.Count);
            Assert.DoesNotContain("Administrator", rolesResult.Roles);
        }

        [Fact]
        public async Task GrantUser_ShouldReturnForbidden_ForEmployees()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            // Act
            var response = await client.GetAsync("/Identity/Roles");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task GrantUser_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.GetAsync("/Identity/Roles");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task RolesEndpoint_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.GetAsync("/Identity/Roles");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
