namespace NutriBest.Server.Tests.Controllers.Newsletter
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using System.Net;
    using System.Text.Json;

    [Collection("Newsletter Controller Tests")]
    public class RemoveSubscriberIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public RemoveSubscriberIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task RemoveSubscriber_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedSubscriber(clientHelper, "user@example.com");

            Assert.NotEmpty(db!.Newsletter);

            // Act
            var response = await client.DeleteAsync("/Newsletter/Admin/RemoveFromNewsletter?email=user@example.com");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            Assert.True(succeeded);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.Newsletter
                         .Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task RemoveSubscriber_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedSubscriber(clientHelper, "user@example.com");

            Assert.NotEmpty(db!.Newsletter);

            // Act
            var response = await client.DeleteAsync("/Newsletter/Admin/RemoveFromNewsletter?email=user@example.com");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            Assert.True(succeeded);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.Newsletter
                         .Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task RemoveSubscriber_ShouldReturnFalse_WhenSubscriberDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedSubscriber(clientHelper, "user@example.com");

            Assert.NotEmpty(db!.Newsletter);

            // Act
            var response = await client.DeleteAsync("/Newsletter/Admin/RemoveFromNewsletter?email=user0@example.com");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            Assert.False(succeeded);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(db!.Newsletter
                         .Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task RemoveSubscriber_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSubscriber(clientHelper, "user@example.com");

            Assert.NotEmpty(db!.Newsletter);

            // Act
            var response = await client.DeleteAsync("/Newsletter/Admin/RemoveFromNewsletter?email=user@example.com");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEmpty(db!.Newsletter
                         .Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task RemoveSubscriber_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSubscriber(clientHelper, "user@example.com");

            Assert.NotEmpty(db!.Newsletter);

            // Act
            var response = await client.DeleteAsync("/Newsletter/Admin/RemoveFromNewsletter?email=user@example.com");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotEmpty(db!.Newsletter
                         .Where(x => !x.IsDeleted));
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
