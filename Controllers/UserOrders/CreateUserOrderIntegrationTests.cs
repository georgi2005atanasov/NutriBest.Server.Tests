using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.UserOrders
{
    using System.Net;
    using System.Text.Json;
    using System.Net.Http.Json;
    using Xunit;
    using Moq;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Carts.Models;
    using NutriBest.Server.Features.Invoices.Models;
    using NutriBest.Server.Features.UsersOrders.Models;
    using Infrastructure.Extensions;
    using static ErrorMessages.UsersOrdersController;
    using static ErrorMessages.OrderDetailsService;

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
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832"
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
            var cart = await db.Carts
                .Include(x => x.CartProducts)
                .FirstAsync(x => x.Id == order.CartId);

            Assert.Null(order.GuestOrderId);
            Assert.Equal(1, order.UserOrderId);

            Assert.Equal("Karlovska", orderDetails.Address!.Street);
            Assert.Equal("900", orderDetails.Address!.StreetNumber);
            Assert.Equal(4000, orderDetails.Address.PostalCode);
            Assert.Equal("Plovdiv", orderDetails.Address.City!.CityName);
            Assert.Equal("Bulgaria", orderDetails.Address.Country.CountryName);
            Assert.False(orderDetails.Address.IsAnonymous);

            Assert.Equal(2, cart.CartProducts.Count);
            Assert.Equal(474.90m, cart.TotalProducts);
            Assert.Equal(474.90m, cart.OriginalPrice);
            Assert.Equal(10, cart.ShippingPrice);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldBeExecuted_WithInvoice()
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
                Flavour = "Cafe Latte",
                Grams = 2000,
                Count = 9,
                Price = 2000.99m,
                ProductId = 7
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
                Name = "TEST USER!!!",
                HasInvoice = true,
                Invoice = new InvoiceServiceModel
                {
                    FirstName = "TEST",
                    LastName = "INVOICE",
                    Bullstat = "123456",
                    CompanyName = "TEST COMPANY",
                    PersonInCharge = "TEST PERSON IN CHARGE",
                    PhoneNumber = "0884138850"
                },
                PaymentMethod = "CashOnDelivery",
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
            Assert.NotEmpty(db!.Invoices.Where(x => x.Id == 1));

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
            var invoice = await db.Invoices
                .FirstAsync();
            var cart = await db.Carts
               .Include(x => x.CartProducts)
               .FirstAsync(x => x.Id == order.CartId);

            Assert.Null(order.GuestOrderId);
            Assert.Equal(1, order.UserOrderId);

            Assert.Equal("Karlovska", orderDetails.Address!.Street);
            Assert.Equal("900", orderDetails.Address!.StreetNumber);
            Assert.Equal(4000, orderDetails.Address.PostalCode);
            Assert.Equal("Plovdiv", orderDetails.Address.City!.CityName);
            Assert.Equal("Bulgaria", orderDetails.Address.Country.CountryName);
            Assert.False(orderDetails.Address.IsAnonymous);

            Assert.Equal("TEST", invoice.FirstName);
            Assert.Equal("INVOICE", invoice.LastName);
            Assert.Equal("123456", invoice.Bullstat);
            Assert.Equal("TEST COMPANY", invoice.CompanyName);
            Assert.Equal("123456", invoice.Bullstat);
            Assert.Equal("TEST PERSON IN CHARGE", invoice.PersonInCharge);
            Assert.Equal("0884138850", invoice.PhoneNumber);

            Assert.Equal(2, cart.CartProducts.Count);
            Assert.Equal(18024.90m, cart.TotalProducts);
            Assert.Equal(18024.90m, cart.OriginalPrice);
            Assert.Equal(10, cart.ShippingPrice);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldBeExecuted_WithProductWithPromotion()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var firstCartProductModel = new CartProductServiceModel
            {
                Flavour = "Cafe Latte",
                Grams = 2000,
                Count = 1,
                Price = 2000.99m,
                ProductId = 7
            };

            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();
            var admin = await clientHelper.GetAdministratorClientAsync();
            await admin.PostAsync("/Promotions", formDataPercentDiscount);
            await admin.PutAsync("/Promotions/Status/1", null);

            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "TEST_EMAIL@example.com",
                Name = "TEST USER!!!",
                HasInvoice = true,
                Invoice = new InvoiceServiceModel
                {
                    FirstName = "TEST",
                    LastName = "INVOICE",
                    Bullstat = "123456",
                    CompanyName = "TEST COMPANY",
                    PersonInCharge = "TEST PERSON IN CHARGE",
                    PhoneNumber = "0884138850"
                },
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
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
            var invoice = await db.Invoices
                .FirstAsync();
            var cart = await db.Carts
                .Include(x => x.CartProducts)
                .FirstAsync(x => x.Id == order.CartId);

            Assert.Equal(1, order.UserOrderId);
            Assert.Null(order.GuestOrderId);

            Assert.Equal("Karlovska", orderDetails.Address!.Street);
            Assert.Equal("900", orderDetails.Address!.StreetNumber);
            Assert.Equal(4000, orderDetails.Address.PostalCode);
            Assert.Equal("Plovdiv", orderDetails.Address.City!.CityName);
            Assert.Equal("Bulgaria", orderDetails.Address.Country.CountryName);
            Assert.False(orderDetails.Address.IsAnonymous);

            Assert.Equal("TEST", invoice.FirstName);
            Assert.Equal("INVOICE", invoice.LastName);
            Assert.Equal("123456", invoice.Bullstat);
            Assert.Equal("TEST COMPANY", invoice.CompanyName);
            Assert.Equal("123456", invoice.Bullstat);
            Assert.Equal("TEST PERSON IN CHARGE", invoice.PersonInCharge);
            Assert.Equal("0884138850", invoice.PhoneNumber);

            Assert.Single(cart.CartProducts);
            Assert.Equal(1500.7425m, cart.TotalProducts);
            Assert.Equal(1500.7425m, cart.OriginalPrice);
            Assert.Equal(500.2475m, cart.TotalSaved);
            Assert.Equal(10, cart.ShippingPrice);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldBeExecutedWithProducts_WithPromotionsAndPromoCodeApplied()
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

            var thirdCartProductModel = new CartProductServiceModel
            {
                Flavour = "Cafe Latte",
                Grams = 2000,
                Count = 3,
                Price = 2000.99m,
                ProductId = 7
            };

            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();
            var admin = await clientHelper.GetAdministratorClientAsync();
            await admin.PostAsync("/Promotions", formDataPercentDiscount);
            await admin.PutAsync("/Promotions/Status/1", null);

            // Act
            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Set", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Second product addition
            var secondResponse = await client.PostAsJsonAsync("/Cart/Set", secondCartProductModel);
            var updatedCookieHeaderAfterSecond = secondResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (updatedCookieHeaderAfterSecond != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", updatedCookieHeaderAfterSecond);
            }

            var thirdResponse = await client.PostAsJsonAsync("/Cart/Set", thirdCartProductModel);
            var updatedCookieHeaderAfterThird = thirdResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (updatedCookieHeaderAfterThird != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", updatedCookieHeaderAfterThird);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "TEST_EMAIL@example.com",
                Name = "TEST USER!!!",
                HasInvoice = true,
                Invoice = new InvoiceServiceModel
                {
                    FirstName = "TEST",
                    LastName = "INVOICE",
                    Bullstat = "123456",
                    CompanyName = "TEST COMPANY",
                    PersonInCharge = "TEST PERSON IN CHARGE",
                    PhoneNumber = "0884138850"
                },
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
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
            var invoice = await db.Invoices
                .FirstAsync();
            var cart = await db.Carts
                .Include(x => x.CartProducts)
                .FirstAsync(x => x.Id == order.CartId);

            Assert.Equal(1, order.UserOrderId);
            Assert.Null(order.GuestOrderId);

            Assert.Equal("Karlovska", orderDetails.Address!.Street);
            Assert.Equal("900", orderDetails.Address!.StreetNumber);
            Assert.Equal(4000, orderDetails.Address.PostalCode);
            Assert.Equal("Plovdiv", orderDetails.Address.City!.CityName);
            Assert.Equal("Bulgaria", orderDetails.Address.Country.CountryName);
            Assert.False(orderDetails.Address.IsAnonymous);

            Assert.Equal("TEST", invoice.FirstName);
            Assert.Equal("INVOICE", invoice.LastName);
            Assert.Equal("123456", invoice.Bullstat);
            Assert.Equal("TEST COMPANY", invoice.CompanyName);
            Assert.Equal("123456", invoice.Bullstat);
            Assert.Equal("TEST PERSON IN CHARGE", invoice.PersonInCharge);
            Assert.Equal("0884138850", invoice.PhoneNumber);

            Assert.Equal(3, cart.CartProducts.Count);
            Assert.Equal(4977.1275m, cart.TotalProducts);
            Assert.Equal(4977.1275m, cart.OriginalPrice);
            Assert.Equal(1500.7425m, cart.TotalSaved);
            Assert.Equal(10, cart.ShippingPrice);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldBeExecuted_WithShippingDiscountApplied()
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

            await SeedingHelper.SeedShippingDiscount(clientHelper,
                "Bulgaria",
                "TEST SHIPPING DISCOUNT 100%!",
                "100",
                null,
                null);

            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
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
            var cart = await db.Carts
                .Include(x => x.CartProducts)
                .FirstAsync(x => x.Id == order.CartId);

            Assert.Null(order.GuestOrderId);
            Assert.Equal(1, order.UserOrderId);

            Assert.Equal("Karlovska", orderDetails.Address!.Street);
            Assert.Equal("900", orderDetails.Address!.StreetNumber);
            Assert.Equal(4000, orderDetails.Address.PostalCode);
            Assert.Equal("Plovdiv", orderDetails.Address.City!.CityName);
            Assert.Equal("Bulgaria", orderDetails.Address.Country.CountryName);
            Assert.False(orderDetails.Address.IsAnonymous);

            Assert.Single(cart.CartProducts);
            Assert.Equal(15.99m, cart.TotalProducts);
            Assert.Equal(15.99m, cart.OriginalPrice);
            Assert.Equal(0, cart.ShippingPrice);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldBeExecuted_WithShippingDiscountNotApplied_SincePriceIsLow()
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

            await SeedingHelper.SeedShippingDiscount(clientHelper,
                "Bulgaria",
                "TEST SHIPPING DISCOUNT 100%!",
                "100",
                null,
                "16");

            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "userInvalid@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
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
            var cart = await db.Carts
                .Include(x => x.CartProducts)
                .FirstAsync(x => x.Id == order.CartId);

            Assert.Null(order.GuestOrderId);
            Assert.Equal(1, order.UserOrderId);

            Assert.Equal("Karlovska", orderDetails.Address!.Street);
            Assert.Equal("900", orderDetails.Address!.StreetNumber);
            Assert.Equal(4000, orderDetails.Address.PostalCode);
            Assert.Equal("Plovdiv", orderDetails.Address.City!.CityName);
            Assert.Equal("Bulgaria", orderDetails.Address.Country.CountryName);
            Assert.False(orderDetails.Address.IsAnonymous);

            Assert.Single(cart.CartProducts);
            Assert.Equal(15.99m, cart.TotalProducts);
            Assert.Equal(15.99m, cart.OriginalPrice);
            Assert.Equal(10, cart.ShippingPrice);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldBeExecuted_AndNotificationShouldAlsoBeSent()
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

            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
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
            var cart = await db.Carts
                .Include(x => x.CartProducts)
                .FirstAsync(x => x.Id == order.CartId);

            Assert.Null(order.GuestOrderId);
            Assert.Equal(1, order.UserOrderId);

            Assert.Equal("Karlovska", orderDetails.Address!.Street);
            Assert.Equal("900", orderDetails.Address!.StreetNumber);
            Assert.Equal(4000, orderDetails.Address.PostalCode);
            Assert.Equal("Plovdiv", orderDetails.Address.City!.CityName);
            Assert.Equal("Bulgaria", orderDetails.Address.Country.CountryName);
            Assert.False(orderDetails.Address.IsAnonymous);

            Assert.Single(cart.CartProducts);
            Assert.Equal(15.99m, cart.TotalProducts);
            Assert.Equal(15.99m, cart.OriginalPrice);
            Assert.Equal(10, cart.ShippingPrice);

            fixture.Factory.NotificationServiceMock!
                .Verify(x => x.SendNotificationToAdmin("success", It.IsAny<string>()));
        }

        [Fact]
        public async Task CreateUserOrder_ShouldReturnBadRequest_SinceCountryIsInvalid()
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

            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Fake country",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
            var data = await orderResponse.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, orderResponse.StatusCode);
            Assert.Equal(WeDoNotShipToThisCountry, result.Message);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldReturnBadRequest_SinceCityIsInvalid()
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

            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Germany",
                City = "Fake City",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
            var data = await orderResponse.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, orderResponse.StatusCode);
            Assert.Equal(WeDoNotShipToThisCity, result.Message);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldReturnBadRequest_SinceCityAndCountryDoesNotMatchd()
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

            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Add", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Germany",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
            var data = await orderResponse.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, orderResponse.StatusCode);
            Assert.Equal(InvalidCityOrCountry, result.Message);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldReturnBadRequest_WhenPaymentMethodIsInvalid()
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
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "InvalidPaymentMethod",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
            var data = await orderResponse.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, orderResponse.StatusCode);
            Assert.Equal(InvalidPaymentMethod, result.Message);
            Assert.Empty(db!.UsersOrders.Where(x => x.Id == 1));
            Assert.Empty(db!.Orders.Where(x => x.Id == 1));
            Assert.Empty(db!.OrdersDetails.Where(x => x.Id == 1));
        }

        [Fact]
        public async Task CreateUserOrder_ShouldReturnBadRequest_WhenPostalCodeIsNaN()
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
                PostalCode = "4000p",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "BankTransfer",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
            var data = await orderResponse.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, orderResponse.StatusCode);
            Assert.Equal(InvalidPostalCode, result.Message);
            Assert.Empty(db!.UsersOrders.Where(x => x.Id == 1));
            Assert.Empty(db!.Orders.Where(x => x.Id == 1));
            Assert.Empty(db!.OrdersDetails.Where(x => x.Id == 1));
        }

        [Fact]
        public async Task CreateUserOrder_ShouldReturnBadRequest_WhenThereAreNoProductsInTheCart()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
            var data = await orderResponse.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, orderResponse.StatusCode);
            Assert.Equal(PurchaseIsRequiredToHaveSomething, result.Message);
            Assert.Empty(db!.UsersOrders.Where(x => x.Id == 1));
            Assert.Empty(db!.Orders.Where(x => x.Id == 1));
            Assert.Empty(db!.OrdersDetails.Where(x => x.Id == 1));
        }

        [Fact]
        public async Task CreateUserOrder_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, orderResponse.StatusCode);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldReturnForbidden_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, orderResponse.StatusCode);
        }

        [Fact]
        public async Task CreateUserOrder_ShouldReturnForbidden_ForAdministrator()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, orderResponse.StatusCode);
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
