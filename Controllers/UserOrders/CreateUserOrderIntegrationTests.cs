namespace NutriBest.Server.Tests.Controllers.UserOrders
{
    using System.Net;
    using System.Text.Json;
    using System.Net.Http.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Carts.Models;
    using NutriBest.Server.Features.Products.Models;
    using NutriBest.Server.Features.UsersOrders.Models;
    using Infrastructure.Extensions;
    using Microsoft.EntityFrameworkCore;

    [Collection("Users Orders Controller Tests")]
    public class CreateUserOrderIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CreateUserOrderIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task CreateUserOrder_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var firstCartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 1,
                Price = 15.99m,
                ProductId = 1
            };

            var secondCartProductModel = new CartProductServiceModel
            {
                Flavour = "Lemon Lime",
                Grams = 500,
                Count = 9,
                Price = 50.99m,
                ProductId = 3
            };

            // Act
            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Second product addition
            var secondResponse = await client.PostAsJsonAsync("/Cart/Add", secondCartProductModel);
            var updatedCookieHeaderAfterSecond = secondResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (updatedCookieHeaderAfterSecond != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", updatedCookieHeaderAfterSecond);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "Pesho Petrov",
                HasInvoice = false,
                PaymentMethod = "CashОnDelivery",
                PhoneNumber = "0884138832",
            };

            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
            orderResponse.EnsureSuccessStatusCode();

            var data = await orderResponse.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            int id = root.GetProperty("id").GetInt32();

            Assert.Equal(1, id);
            Assert.NotEmpty(db!.UsersOrders.Where(x => x.Id == 1));
            Assert.NotEmpty(db!.Orders.Where(x => x.Id == 1));
            Assert.NotEmpty(db!.OrdersDetails.Where(x => x.Id == 1));

            var order = await db.Orders
                .FirstAsync();
            var orderDetails = await db.OrdersDetails
            .Include(x => x.Address)
                .ThenInclude(address => address!.City)
            .Include(x => x.Address)
                .ThenInclude(address => address!.Country)
            .FirstAsync();
            var userOrder = await db.UsersOrders
                .FirstAsync();

            Assert.Null(order.GuestOrderId);
            Assert.Equal(1, order.UserOrderId);
            Assert.Equal("Karlovska", orderDetails.Address!.Street);
            Assert.Equal("900", orderDetails.Address!.StreetNumber);
            Assert.Equal(4000, orderDetails.Address.PostalCode);
            Assert.Equal("Plovdiv", orderDetails.Address.City!.CityName);
            Assert.Equal("Bulgaria", orderDetails.Address.Country.CountryName);
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
