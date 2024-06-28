namespace NutriBest.Server.Tests.Controllers.Home
{
    using Xunit;
    using System.Text.Json;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Features.Home.Models;
    using Microsoft.Extensions.Configuration;

    [Collection("Home Controller Tests")]
    public class HomeControllerIntegrationTests : IAsyncLifetime
    {
        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IConfiguration config;

        public HomeControllerIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
            var scope = fixture.Factory.Services.CreateScope();
            config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
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

            var expectedPhoneNumber = config.GetValue<string>("Admin:PhoneNumber");
            var expectedEmail = config.GetValue<string>("Admin:Email");

            Assert.Equal("", result.Address);
            Assert.Equal(expectedPhoneNumber, result.PhoneNumber);
            Assert.Equal(expectedEmail, result.Email);
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
