namespace NutriBest.Server.Tests.Controllers.Orders
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Orders.Models;
    using Infrastructure.Extensions;
    using System.Net;

    [Collection("Orders Controller Tests")]
    public class GetOrderFromAdminIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public GetOrderFromAdminIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task GetUserOrderFromAdmin_ShouldBeExecuted_ForAdmin()
        {
            // Arrange 
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedUserOrder(clientHelper, 
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

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
            Assert.Equal("TEST USER!!!", result.CustomerName);
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

        [Fact]
        public async Task GetUserOrderFromAdmin_ShouldReturnNull_SinceOrderIsInvalid()
        {
            // Arrange 
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedUserOrder(clientHelper, 
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            // Act
            var response = await client.GetAsync("/Orders/Admin/2");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", data);
        }

        [Fact]
        public async Task GetUserOrderFromAdmin_ShouldBeExecuted_ForEmployee()
        {
            // Arrange 
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedUserOrder(clientHelper, 
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

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
            Assert.Equal("TEST USER!!!", result.CustomerName);
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

        [Fact]
        public async Task GetUserOrderFromAdmin_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange 
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedUserOrder(clientHelper, 
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            // Act
            var response = await client.GetAsync("/Orders/Admin/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("", data);
        }

        [Fact]
        public async Task GetUserOrderFromAdmin_ShouldReturnForbidden_ForUsers()
        {
            // Arrange 
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedUserOrder(clientHelper, 
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            // Act
            var response = await client.GetAsync("/Orders/Admin/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Equal("", data);
        }

        [Fact]
        public async Task GetGuestOrderFromAdmin_ShouldBeExecuted_ForAdmin()
        {
            // Arrange 
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedGuestOrder(clientHelper);

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
            Assert.Equal("TEST_EMAIL@example.com", result.Email);
            Assert.Equal("TEST USER!!!", result.CustomerName);
            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("900", result.StreetNumber);
        }

        [Fact]
        public async Task GetGuestOrderFromAdmin_ShouldReturnNull_SinceOrderIsInvalid()
        {
            // Arrange 
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedGuestOrder(clientHelper);

            // Act
            var response = await client.GetAsync("/Orders/Admin/2");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", data);
        }

        [Fact]
        public async Task GetGuestOrderFromAdmin_ShouldBeExecuted_ForEmployee()
        {
            // Arrange 
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedGuestOrder(clientHelper);

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
            Assert.Equal("TEST_EMAIL@example.com", result.Email);
            Assert.Equal("TEST USER!!!", result.CustomerName);
            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("900", result.StreetNumber);
        }

        [Fact]
        public async Task GetGuestOrderFromAdmin_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange 
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedUserOrder(clientHelper, 
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            // Act
            var response = await client.GetAsync("/Orders/Admin/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("", data);
        }

        [Fact]
        public async Task GetGuestOrderFromAdmin_ShouldReturnForbidden_ForUsers()
        {
            // Arrange 
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedUserOrder(clientHelper, 
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            // Act
            var response = await client.GetAsync("/Orders/Admin/1");
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
