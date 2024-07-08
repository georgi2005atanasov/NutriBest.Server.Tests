namespace NutriBest.Server.Tests.Controllers.Orders
{
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Orders.Models;
    using Infrastructure.Extensions;
    using System.Net;
    using System.Net.Http.Json;
    using NutriBest.Server.Features.Carts.Models;
    using NutriBest.Server.Features.GuestsOrders.Models;
    using System.Web;
    using NutriBest.Server.Features.UsersOrders.Models;
    using NutriBest.Server.Features.Invoices.Models;
    using Moq;
    using NutriBest.Server.Features.Identity.Models;

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
        public async Task GetOrder_ShouldBeExecutedAndReturnNull_WhenOrderIsInvalid()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.GetAsync($"/Orders/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Equal("", data);
        }

        [Fact]
        public async Task GetGuestOrder_ShouldBeExecuted()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

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

            var orderModel = new GuestOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "TEST_EMAIL@example.com",
                Name = "TEST_USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/GuestsOrders", orderModel);
            cookieHeader = orderResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Act
            var response = await client.GetAsync($"/Orders/1");
            var data = await response.Content.ReadAsStringAsync();
          
            // Assert
            var result = JsonSerializer.Deserialize<OrderServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new OrderServiceModel();

            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("900", result.StreetNumber);
            Assert.Equal("TEST_EMAIL@example.com", result.Email);
            Assert.Equal("TEST_USER!!!", result.CustomerName);
            Assert.Equal("CashOnDelivery", result.PaymentMethod);
            Assert.Equal("0884138832", result.PhoneNumber);
        }

        [Fact]
        public async Task GetGuestOrder_ShouldBeExecuted_WithInvoice()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

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

            var orderModel = new GuestOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "TEST_EMAIL@example.com",
                Name = "TEST_USER!!!",
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
            var orderResponse = await client.PostAsJsonAsync("/GuestsOrders", orderModel);
            cookieHeader = orderResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Act
            var response = await client.GetAsync($"/Orders/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<OrderServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new OrderServiceModel();

            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("900", result.StreetNumber);
            Assert.Equal("TEST_EMAIL@example.com", result.Email);
            Assert.Equal("TEST_USER!!!", result.CustomerName);
            Assert.Equal("CashOnDelivery", result.PaymentMethod);
            Assert.Equal("0884138832", result.PhoneNumber);

            Assert.Equal("TEST", result.Invoice!.FirstName);
            Assert.Equal("INVOICE", result.Invoice!.LastName);
            Assert.Equal("123456", result.Invoice!.Bullstat);
            Assert.Equal("TEST COMPANY", result.Invoice!.CompanyName);
            Assert.Equal("123456", result.Invoice!.Bullstat);
            Assert.Equal("TEST PERSON IN CHARGE", result.Invoice!.PersonInCharge);
            Assert.Equal("0884138850", result.Invoice!.PhoneNumber);
        }

        [Fact]
        public async Task GetGuestOrder_ShouldReturnBadrequest_SinceTheTokenIsInvalid()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

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

            var orderModel = new GuestOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "TEST_EMAIL@example.com",
                Name = "TEST_USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            await client.PostAsJsonAsync("/GuestsOrders", orderModel);
            
            // Act
            var response = await client.GetAsync($"/Orders/1");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetGuestOrder_ShouldReturnBadrequest_SinceClientIsUser()
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

            var orderModel = new GuestOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "TEST_EMAIL@example.com",
                Name = "TEST_USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            var orderResponse = await client.PostAsJsonAsync("/GuestsOrders", orderModel);
            cookieHeader = orderResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Act
            var response = await client.GetAsync($"/Orders/1");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetUserOrder_ShouldBeExecuted()
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
                Email = "TEST_EMAIL@example.com",
                Name = "TEST_USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
            cookieHeader = orderResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Act
            var response = await client.GetAsync("/Orders/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<OrderServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new OrderServiceModel();

            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("900", result.StreetNumber);
            Assert.Equal("TEST_EMAIL@example.com", result.Email);
            Assert.Equal("TEST_USER!!!", result.CustomerName);
            Assert.Equal("CashOnDelivery", result.PaymentMethod);
            Assert.Equal("0884138832", result.PhoneNumber);
        }

        [Fact]
        public async Task GetUserOrder_ShouldBeExecuted_WithInvoice()
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
                Email = "TEST_EMAIL@example.com",
                Name = "TEST_USER!!!",
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
            cookieHeader = orderResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Act
            var response = await client.GetAsync("/Orders/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<OrderServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new OrderServiceModel();

            Assert.Equal("Bulgaria", result.Country);
            Assert.Equal("Plovdiv", result.City);
            Assert.Equal("Karlovska", result.Street);
            Assert.Equal("900", result.StreetNumber);
            Assert.Equal("TEST_EMAIL@example.com", result.Email);
            Assert.Equal("TEST_USER!!!", result.CustomerName);
            Assert.Equal("CashOnDelivery", result.PaymentMethod);
            Assert.Equal("0884138832", result.PhoneNumber);

            Assert.Equal("TEST", result.Invoice!.FirstName);
            Assert.Equal("INVOICE", result.Invoice!.LastName);
            Assert.Equal("123456", result.Invoice!.Bullstat);
            Assert.Equal("TEST COMPANY", result.Invoice!.CompanyName);
            Assert.Equal("123456", result.Invoice!.Bullstat);
            Assert.Equal("TEST PERSON IN CHARGE", result.Invoice!.PersonInCharge);
            Assert.Equal("0884138850", result.Invoice!.PhoneNumber);
        }

        [Fact]
        public async Task GetUserOrder_ShouldReturnBadRequest_WhenUserIsNotTheSame()
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

            // Seed another user
            await SeedingHelper.SeedUser(clientHelper,
                "pesho",
                "pesho@example.com",
                "Pesho12345",
                "Pesho12345");

            // Simulates user who got the cookie
            var hacker = await clientHelper.GetAuthenticatedClientAsync("pesho", "Pesho12345");

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
                Name = "TEST_USER!!!",
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
            cookieHeader = orderResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();

            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
                hacker.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Act
            var response = await hacker.GetAsync("/Orders/1");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
