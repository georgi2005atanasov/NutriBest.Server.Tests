namespace NutriBest.Server.Tests.Controllers.Home
{
    using Xunit;
    using System.Text.Json;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Features.Home.Models;

    [Collection("Home Controller Tests")]
    public class HomeControllerIntegrationTests : IAsyncLifetime
    {
        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        public HomeControllerIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task ContactUs_ShouldReturnContactDetails()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.GetAsync("/Home/ContactUs");
            var data = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ContactUsInfoServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new ContactUsInfoServiceModel();

            Assert.Equal("", result.Address);
            Assert.Equal("+359884138832", result.PhoneNumber);
            Assert.Equal("admin@example.com", result.Email);
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
