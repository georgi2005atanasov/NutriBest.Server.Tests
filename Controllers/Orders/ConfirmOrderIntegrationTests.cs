using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Orders
{
    using System.Net;
    using System.Text.Json;
    using Moq;
    using Xunit;
    using AutoMapper;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Features.Notifications.Hubs;
    using Infrastructure.Extensions;
    using static ErrorMessages.NotificationService;

    [Collection("Orders Controller Tests")]
    public class ConfirmOrderIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public ConfirmOrderIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task ConfirmOrder_ShouldBeExecuted()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedUserOrder(clientHelper, false);

            // Act
            var response = await client.PostAsync("/Orders/Confirm?orderId=1", null);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool hasUpdated = root.GetProperty("hasUpdated").GetBoolean();

            Assert.True(hasUpdated);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var order = await db!.Orders
                .FirstAsync(x => x.Id == 1);
            Assert.True(order.IsConfirmed);
        }

        [Fact]
        public async Task ConfirmOrder_ShouldBeExecuted_AndShouldAlsoSendLowInStockNotifications_LowPriority()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper
                .SeedUserOrderForSendingLowStockNotification(clientHelper, 81);

            // Act
            var response = await client.PostAsync("/Orders/Confirm?orderId=1", null);
            var data = await response.Content.ReadAsStringAsync();

            // Ensure Notification Service gets executed
            fixture.Factory.NotificationServiceMock!
                .Verify(x => x.SendLowInStockNotification("product80", 1, 19, "#0000001"), Times.Once);

            // Artificially make the notification service and seed the notification
            var mockClientProxy = new Mock<IClientProxy>();
            mockClientProxy
                .Setup(x => x.SendCoreAsync(It.IsAny<string>(),
                It.IsAny<object[]>(),
                default))
                .Returns(Task.CompletedTask);

            var mockClients = new Mock<IHubClients>();
            mockClients
                .Setup(clients => clients.All)
                .Returns(mockClientProxy.Object);

            var mockHubContext = new Mock<IHubContext<NotificationHub>>();
            mockHubContext
                .Setup(context => context.Clients)
                .Returns(mockClients.Object);

            var notificationService = new NotificationService(mockHubContext.Object,
                db!,
                new Mock<IMapper>().Object);
            await notificationService
                .SendLowInStockNotification("product80", 1, 19, "#0000001");

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool hasUpdated = root.GetProperty("hasUpdated").GetBoolean();

            Assert.True(hasUpdated);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var order = await db!.Orders
                .FirstAsync(x => x.Id == 1);

            Assert.True(order.IsConfirmed);

            var notification = await db.Notifications
                .FirstOrDefaultAsync(x => x.ProductId == 1);

            Assert.NotNull(notification);
            Assert.Equal(LowInStock, notification!.Title);
            Assert.Equal(Data.Enums.Priority.Low, notification.Priority);
            Assert.Equal("Be Aware That Product With Name 'product80' has Quantity of 19.", notification.Message);
        }

        [Fact]
        public async Task ConfirmOrder_ShouldBeExecuted_AndShouldAlsoSendLowInStockNotifications_MediumPriority()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper
                .SeedUserOrderForSendingLowStockNotification(clientHelper, 91);

            // Act
            var response = await client.PostAsync("/Orders/Confirm?orderId=1", null);
            var data = await response.Content.ReadAsStringAsync();

            // Ensure Notification Service gets executed
            fixture.Factory.NotificationServiceMock!
                .Verify(x => x.SendLowInStockNotification("product80", 1, 9, "#0000001"), Times.Once);

            // Artificially make the notification service and seed the notification
            var mockClientProxy = new Mock<IClientProxy>();
            mockClientProxy
                .Setup(x => x.SendCoreAsync(It.IsAny<string>(),
                It.IsAny<object[]>(),
                default))
                .Returns(Task.CompletedTask);

            var mockClients = new Mock<IHubClients>();
            mockClients
                .Setup(clients => clients.All)
                .Returns(mockClientProxy.Object);

            var mockHubContext = new Mock<IHubContext<NotificationHub>>();
            mockHubContext
                .Setup(context => context.Clients)
                .Returns(mockClients.Object);

            var notificationService = new NotificationService(mockHubContext.Object,
                db!,
                new Mock<IMapper>().Object);

            await notificationService
                .SendLowInStockNotification("product80", 1, 9, "#0000001");

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool hasUpdated = root.GetProperty("hasUpdated").GetBoolean();

            Assert.True(hasUpdated);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var order = await db!.Orders
                .FirstAsync(x => x.Id == 1);

            Assert.True(order.IsConfirmed);

            var notification = await db.Notifications
                .FirstOrDefaultAsync(x => x.ProductId == 1);

            Assert.NotNull(notification);
            Assert.Equal(StockIsRunningLow, notification!.Title);
            Assert.Equal(Data.Enums.Priority.Medium, notification.Priority);
            Assert.Equal("'product80' stock levels are critically low! (9 left)", notification.Message);
        }

        [Fact]
        public async Task ConfirmOrder_ShouldBeExecuted_AndShouldAlsoSendLowInStockNotifications_HighPriority()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper
                .SeedUserOrderForSendingLowStockNotification(clientHelper, 100);

            // Act
            var response = await client.PostAsync("/Orders/Confirm?orderId=1", null);
            var data = await response.Content.ReadAsStringAsync();

            // Ensure Notification Service gets executed
            fixture.Factory.NotificationServiceMock!
                .Verify(x => x.SendLowInStockNotification("product80", 1, 0, "#0000001"), Times.Once);

            // Artificially make the notification service and seed the notification
            var mockClientProxy = new Mock<IClientProxy>();
            mockClientProxy
                .Setup(x => x.SendCoreAsync(It.IsAny<string>(),
                It.IsAny<object[]>(),
                default))
                .Returns(Task.CompletedTask);

            var mockClients = new Mock<IHubClients>();
            mockClients
                .Setup(clients => clients.All)
                .Returns(mockClientProxy.Object);

            var mockHubContext = new Mock<IHubContext<NotificationHub>>();
            mockHubContext
                .Setup(context => context.Clients)
                .Returns(mockClients.Object);

            var notificationService = new NotificationService(mockHubContext.Object,
                db!,
                new Mock<IMapper>().Object);

            await notificationService
                .SendLowInStockNotification("product80", 1, 0, "#0000001");

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool hasUpdated = root.GetProperty("hasUpdated").GetBoolean();

            Assert.True(hasUpdated);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var order = await db!.Orders
                .FirstAsync(x => x.Id == 1);

            Assert.True(order.IsConfirmed);

            var notification = await db.Notifications
                .FirstOrDefaultAsync(x => x.ProductId == 1);

            Assert.NotNull(notification);
            Assert.Equal(OutOfStock, notification!.Title);
            Assert.Equal(Data.Enums.Priority.High, notification.Priority);
            Assert.Equal("'product80' is Out of Stock! (0)", notification.Message);
        }

        [Fact]
        public async Task ConfirmOrder_ShouldThrowException_ForHighPriority()
        {
            // Artificially make the notification service and seed the notification
            var mockClientProxy = new Mock<IClientProxy>();
            mockClientProxy
                .Setup(x => x.SendCoreAsync(It.IsAny<string>(), It.IsAny<object[]>(), default))
                .Returns(Task.CompletedTask);

            var mockClients = new Mock<IHubClients>();
            mockClients
                .Setup(clients => clients.All)
                .Returns(mockClientProxy.Object);

            var mockHubContext = new Mock<IHubContext<NotificationHub>>();
            mockHubContext
                .Setup(context => context.Clients)
                .Returns(mockClients.Object);

            var notificationService = new NotificationService(mockHubContext.Object,
                db!,
                new Mock<IMapper>().Object);

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await notificationService
            .SendLowInStockNotification("product80", 1, -1, "#0000001"));
        }

        [Fact]
        public async Task ConfirmOrder_ShouldReturnFalse_WhenOrderIsAlreadyConfirmed()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await SeedingHelper.SeedUserOrder(clientHelper, false);

            // Act
            await client.PostAsync("/Orders/Confirm?orderId=1", null);
            var response = await client.PostAsync("/Orders/Confirm?orderId=1", null);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            using JsonDocument document = JsonDocument.Parse(data);
            JsonElement root = document.RootElement;
            bool hasUpdated = root.GetProperty("hasUpdated").GetBoolean();

            Assert.False(hasUpdated);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var order = await db!.Orders
                .FirstAsync(x => x.Id == 1);
            Assert.True(order.IsConfirmed);
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
