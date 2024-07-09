namespace NutriBest.Server.Tests.Controllers.Orders
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Carts.Models;
    using NutriBest.Server.Features.Orders.Models;
    using NutriBest.Server.Features.UsersOrders.Models;
    using NutriBest.Server.Features.GuestsOrders.Models;
    using NutriBest.Server.Tests.Controllers.Orders.Data;
    using Infrastructure.Extensions;

    [Collection("Orders Controller Tests")]
    public class AllOrdersIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public AllOrdersIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllOrdersEndpoint_ShouldBeExecuted_WithoutFilters_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUserOrders(db!,
                clientHelper,
                30,
                new List<CartProductServiceModel>
                {
                    new CartProductServiceModel
                    {
                        Flavour = "Coconut",
                        Grams = 1000,
                        Count = 1,
                        Price = 15.99m,
                        ProductId = 1
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Lemon Lime",
                        Grams = 500,
                        Count = 2,
                        Price = 50.99m,
                        ProductId = 3
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Chocolate",
                        Grams = 500,
                        Count = 3,
                        Price = 500.99m,
                        ProductId = 6
                    }
                },
                new UserOrderServiceModel
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
                });

            // Act
            var response = await client.GetAsync("/Orders?page=1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllOrdersServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllOrdersServiceModel();

            Assert.Equal(180, result.TotalProducts);
            Assert.Equal(3315.4340m, result.TotalDiscounts);
            Assert.Equal(45612.7660m, result.TotalPrice);
            Assert.Equal(48928.2000m, result.TotalPriceWithoutDiscount);
            Assert.Equal(30, result.TotalOrders);
            Assert.Equal(20, result.Orders.Count); // Pagination
            Assert.Equal(result.Orders, result.Orders
                                .OrderByDescending(x => x.MadeOn));
        }

        [Fact]
        public async Task AllOrdersEndpoint_ShouldBeExecuted_WithoutFilters_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await LargeSeedingHelper.SeedUserOrders(db!,
                clientHelper,
                30,
                new List<CartProductServiceModel>
                {
                    new CartProductServiceModel
                    {
                        Flavour = "Coconut",
                        Grams = 1000,
                        Count = 1,
                        Price = 15.99m,
                        ProductId = 1
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Lemon Lime",
                        Grams = 500,
                        Count = 2,
                        Price = 50.99m,
                        ProductId = 3
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Chocolate",
                        Grams = 500,
                        Count = 3,
                        Price = 500.99m,
                        ProductId = 6
                    }
                },
                new UserOrderServiceModel
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
                });

            // Act
            var response = await client.GetAsync("/Orders?page=1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllOrdersServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllOrdersServiceModel();

            Assert.Equal(180, result.TotalProducts);
            Assert.Equal(3315.4340m, result.TotalDiscounts);
            Assert.Equal(45612.7660m, result.TotalPrice);
            Assert.Equal(48928.2000m, result.TotalPriceWithoutDiscount);
            Assert.Equal(30, result.TotalOrders);
            Assert.Equal(20, result.Orders.Count); // Pagination
            Assert.Equal(result.Orders, result.Orders
                                .OrderByDescending(x => x.MadeOn));
        }

        [Fact]
        public async Task AllOrdersEndpoint_ShouldReturnNineteenOrders_OnTheSecondPage()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUserOrders(db!,
                clientHelper,
                30,
                new List<CartProductServiceModel>
                {
                    new CartProductServiceModel
                    {
                        Flavour = "Coconut",
                        Grams = 1000,
                        Count = 1,
                        Price = 15.99m,
                        ProductId = 1
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Lemon Lime",
                        Grams = 500,
                        Count = 2,
                        Price = 50.99m,
                        ProductId = 3
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Chocolate",
                        Grams = 500,
                        Count = 3,
                        Price = 500.99m,
                        ProductId = 6
                    }
                },
                new UserOrderServiceModel
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
                });

            await LargeSeedingHelper.SeedGuestOrders(db!,
                clientHelper,
                9,
                new List<CartProductServiceModel>
                {
                    new CartProductServiceModel
                    {
                        Flavour = "Coconut",
                        Grams = 1000,
                        Count = 1,
                        Price = 15.99m,
                        ProductId = 1
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Lemon Lime",
                        Grams = 500,
                        Count = 2,
                        Price = 50.99m,
                        ProductId = 3
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Chocolate",
                        Grams = 500,
                        Count = 3,
                        Price = 500.99m,
                        ProductId = 6
                    }
                },
                new GuestOrderServiceModel
                {
                    Country = "Bulgaria",
                    City = "Plovdiv",
                    Street = "Karlovska",
                    StreetNumber = "900",
                    PostalCode = "4000",
                    Email = "TEST_USER@example.com",
                    Name = "TEST USER!!!",
                    HasInvoice = false,
                    PaymentMethod = "CashOnDelivery",
                    PhoneNumber = "0884138832"
                });

            // Act
            var response = await client.GetAsync("/Orders?page=2");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllOrdersServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllOrdersServiceModel();

            Assert.Equal(4310.3840m, result.TotalDiscounts);
            Assert.Equal(234, result.TotalProducts);
            Assert.Equal(59296.2760m, result.TotalPrice);
            Assert.Equal(63606.6600m, result.TotalPriceWithoutDiscount);
            Assert.Equal(39, result.TotalOrders);
            Assert.Equal(19, result.Orders.Count); // Pagination
            Assert.Equal(result.Orders, result.Orders
                                .OrderByDescending(x => x.MadeOn));
        }

        [Theory]
        [MemberData(nameof(OrderTestData.GetOrderData), MemberType = typeof(OrderTestData))]
        public async Task AllOrdersEndpoint_ShouldBeExecuted_WithFilters(string? search,
            int page,
            string? filters,
            string? startDate,
            string? endDate,
            int expectedCount)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUserOrders(db!,
                clientHelper,
                30,
                new List<CartProductServiceModel>
                {
                    new CartProductServiceModel
                    {
                        Flavour = "Coconut",
                        Grams = 1000,
                        Count = 1,
                        Price = 15.99m,
                        ProductId = 1
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Lemon Lime",
                        Grams = 500,
                        Count = 2,
                        Price = 50.99m,
                        ProductId = 3
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Chocolate",
                        Grams = 500,
                        Count = 3,
                        Price = 500.99m,
                        ProductId = 6
                    }
                },
                new UserOrderServiceModel
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
                });

            // Act
            var response = await client.GetAsync($"/Orders?page={page}&filters={filters}&startDate={startDate ?? ""}&endDate={endDate ?? ""}&search={search}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<AllOrdersServiceModel>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new AllOrdersServiceModel();

            var confirmed = result.Orders
                .Where(x => x.IsConfirmed)
                .Count();

            var shippedFinished = result.Orders
                .Where(x => x.IsShipped && x.IsFinished)
                .Count();

            var confirmedPaid = result.Orders
                .Where(x => x.IsConfirmed && x.IsPaid)
                .Count();

            Assert.Equal(expectedCount, result.Orders.Count);
        }

        [Fact]
        public async Task AllOrdersEndpoint_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.GetAsync("/Orders?page=1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task AllOrdersEndpoint_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            
            // Act
            var response = await client.GetAsync("/Orders?page=1");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
