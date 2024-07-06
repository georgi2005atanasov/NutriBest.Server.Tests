namespace NutriBest.Server.Tests.Controllers.Newsletter
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using System.Net;
    using System.Text.Json;
    using NutriBest.Server.Shared.Responses;

    [Collection("Newsletter Controller Tests")]
    public class AddSubscriberIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AddSubscriberIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AddSubscriber_ShouldBeExecuted()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var formData = new MultipartFormDataContent
            {
                { new StringContent("gogida9876@abv.bg"), "email" },
            };

            Assert.Empty(db!.Newsletter);

            // Act
            var response = await client.PostAsync("/Newsletter", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("1", data);
            Assert.NotEmpty(db!.Newsletter);
        }

        [Fact]
        public async Task AddSubscriber_ShouldReturnBadRequest_ForAlreadyExistingSubscriber()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var formData = new MultipartFormDataContent
            {
                { new StringContent("gogida9876@abv.bg"), "email" },
            };

            Assert.Empty(db!.Newsletter);

            // Act
            await client.PostAsync("/Newsletter", formData);
            var response = await client.PostAsync("/Newsletter", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("'gogida9876@abv.bg' is already subscribed!", result.Message);
        }

        [Fact]
        public async Task AddSubscriber_ShouldBeExecuted_AndShouldAlsoSendNotification()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var formData = new MultipartFormDataContent
            {
                { new StringContent("gogida9876@abv.bg"), "email" },
            };

            Assert.Empty(db!.Newsletter);

            // Act
            await client.PostAsync("/Newsletter", formData);
            var response = await client.PostAsync("/Newsletter", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            fixture.Factory.NotificationServiceMock!
                .Verify(x => x.SendNotificationToAdmin("success", $"'gogida9876@abv.bg' Has Just Signed up For the Newsletter"));
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
