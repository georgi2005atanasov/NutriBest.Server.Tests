namespace NutriBest.Server.Tests.Controllers.Newsletter
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;

    [Collection("Newsletter Controller Tests")] 
    public class UnsubscribeIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public UnsubscribeIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task Unsubscribe_ShouldBeExecuted_AndShouldReturnTrue()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var formData = new MultipartFormDataContent
            {
                { new StringContent("gogida9876@abv.bg"), "email" }
            };

            await client.PostAsync("/Newsletter", formData);

            // Act
            var response = await client.DeleteAsync($"/Newsletter/Unsubscribe?email={db!.Newsletter.First().Email}&token={db!.Newsletter.First().VerificationToken!}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            Assert.True(succeeded);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.Newsletter.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task Unsubscribe_ShouldReturnFalse_WhenInvalidTokenIsPassed()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var formData = new MultipartFormDataContent
            {
                { new StringContent("gogida9876@abv.bg"), "email" }
            };

            await client.PostAsync("/Newsletter", formData);

            // Act
            var response = await client.DeleteAsync($"/Newsletter/Unsubscribe?email={db!.Newsletter.First().Email}&token=invalidToken");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            Assert.False(succeeded);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(db!.Newsletter.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task Unsubscribe_ShouldReturnFalse_WhenInvalidUserIsPassed()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var formData = new MultipartFormDataContent
            {
                { new StringContent("gogida9876@abv.bg"), "email" }
            };

            await client.PostAsync("/Newsletter", formData);

            // Act
            var response = await client.DeleteAsync($"/Newsletter/Unsubscribe?email=invalidEmail@aaa.com&token={db!.Newsletter.First().VerificationToken}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            Assert.False(succeeded);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotEmpty(db!.Newsletter.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task Unsubscribe_ShouldReturnFalse_WhenUserIsAlreadyDeleted()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var formData = new MultipartFormDataContent
            {
                { new StringContent("gogida9876@abv.bg"), "email" }
            };

            await client.PostAsync("/Newsletter", formData);

            // Act
            await client.DeleteAsync($"/Newsletter/Unsubscribe?email=invalidEmail@aaa.com&token={db!.Newsletter.First().VerificationToken}");
            var response = await client.DeleteAsync($"/Newsletter/Unsubscribe?email=invalidEmail@aaa.com&token={db!.Newsletter.First().VerificationToken}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool succeeded = root.GetProperty("succeeded").GetBoolean();

            Assert.False(succeeded);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
