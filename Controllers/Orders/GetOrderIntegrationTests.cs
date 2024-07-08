namespace NutriBest.Server.Tests.Controllers.Orders
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Orders.Models;
    using Infrastructure.Extensions;

    [Collection("Orders Controller Tests")]
    public class GetOrderIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public GetOrderIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetOrderFromAdmin_ShouldBeExecuted_ForAdmin()
        {
            // Arrange 
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedUserOrder(clientHelper, true);

            // Act
            var response = await client.GetAsync("/Orders/Admin/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<OrderServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new OrderServiceModel();

            Assert.Equal("0884138832", result.PhoneNumber);
            Assert.Equal("CashOnDelivery", result.PaymentMethod);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal("user@example.com", result.CustomerName);
            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("900", result.StreetNumber);

            Assert.Equal("0884138850", result.Invoice!.PhoneNumber);
            Assert.Equal("TEST PERSON IN CHARGE", result.Invoice!.PersonInCharge);
            Assert.Equal("TEST COMPANY", result.Invoice!.CompanyName);
            Assert.Equal("123456", result.Invoice!.Bullstat);
            Assert.Equal("TEST", result.Invoice!.FirstName);
            Assert.Equal("INVOICE", result.Invoice!.LastName);


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
