using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Orders
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
    using NutriBest.Server.Features.Orders.Models;
    using static SuccessMessages.NotificationService;

    [Collection("Orders Controller Tests")]
    public class ChangeStatusesIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public ChangeStatusesIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task ChangeStatus_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedUserOrder(clientHelper);

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = false
            };

            // Act
            var response = await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool isSuccessful = root.GetProperty("successful").GetBoolean();

            Assert.True(isSuccessful);
            var order = await db!.Orders.FirstAsync();
            var orderDetails = await db!.OrdersDetails
                .FirstAsync(x => x.Id == order.Id);
            Assert.True(order.IsConfirmed);
            Assert.True(orderDetails.IsPaid);
            Assert.True(orderDetails.IsShipped);
            Assert.False(order.IsFinished);
        }

        [Fact]
        public async Task ChangeStatus_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedUserOrder(clientHelper);

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            // Act
            var response = await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool isSuccessful = root.GetProperty("successful").GetBoolean();

            Assert.True(isSuccessful);
            var order = await db!.Orders.FirstAsync();
            var orderDetails = await db!.OrdersDetails
                .FirstAsync(x => x.Id == order.Id);
            Assert.True(order.IsConfirmed);
            Assert.True(orderDetails.IsPaid);
            Assert.True(orderDetails.IsShipped);
            Assert.True(order.IsFinished);
        }

        [Fact]
        public async Task ChangeStatus_ShouldBeExecuted_AndShouldAlsoSendNotificationToAdmin()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedUserOrder(clientHelper);

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            // Act
            var response = await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool isSuccessful = root.GetProperty("successful").GetBoolean();

            Assert.True(isSuccessful);
            var order = await db!.Orders.FirstAsync();
            var orderDetails = await db!.OrdersDetails
                .FirstAsync(x => x.Id == order.Id);
            Assert.True(order.IsConfirmed);
            Assert.True(orderDetails.IsPaid);
            Assert.True(orderDetails.IsShipped);
            Assert.True(order.IsFinished);

            fixture.Factory.NotificationServiceMock!
                .Verify(x => x.SendNotificationToAdmin("success", string.Format(OrderHasJustBeenConfirmed, 1)));
        }

        [Fact]
        public async Task ChangeStatus_ShouldReturnFalse_SinceOrderDoesNotExists()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedUserOrder(clientHelper);

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            // Act
            var response = await client.PutAsJsonAsync("/Orders/ChangeStatus/2", statusesModel);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool isSuccessful = root.GetProperty("successful").GetBoolean();

            Assert.False(isSuccessful);
            var order = await db!.Orders.FirstAsync();
            var orderDetails = await db!.OrdersDetails
                .FirstAsync(x => x.Id == order.Id);
            Assert.False(order.IsConfirmed);
            Assert.False(orderDetails.IsPaid);
            Assert.False(orderDetails.IsShipped);
            Assert.False(order.IsFinished);
        }

        [Fact]
        public async Task ChangeStatus_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedUserOrder(clientHelper);

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            // Act
            var response = await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var order = await db!.Orders.FirstAsync();
            var orderDetails = await db!.OrdersDetails
                .FirstAsync(x => x.Id == order.Id);
            Assert.False(order.IsConfirmed);
            Assert.False(orderDetails.IsPaid);
            Assert.False(orderDetails.IsShipped);
            Assert.False(order.IsFinished);
        }

        [Fact]
        public async Task ChangeStatus_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedUserOrder(clientHelper);

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            // Act
            var response = await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            var order = await db!.Orders.FirstAsync();
            var orderDetails = await db!.OrdersDetails
                .FirstAsync(x => x.Id == order.Id);
            Assert.False(order.IsConfirmed);
            Assert.False(orderDetails.IsPaid);
            Assert.False(orderDetails.IsShipped);
            Assert.False(order.IsFinished);
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
