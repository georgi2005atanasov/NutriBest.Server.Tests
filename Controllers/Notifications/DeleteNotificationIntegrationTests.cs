namespace NutriBest.Server.Tests.Controllers.Notifications
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using System.Net;
    using System.Text.Json;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Infrastructure.Extensions;
    using Moq;
    using Microsoft.AspNetCore.SignalR;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Features.Notifications.Hubs;
    using AutoMapper;

    [Collection("Notifications Controller Tests")]
    public class DeleteNotificationIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public DeleteNotificationIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task DeleteNotification_ShouldBeExecuted()
        {
            // Arrange
            await SeedingHelper.SeedThreeProducts(clientHelper);
            await SeedingHelper.SeedSevenProducts(clientHelper);

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

            for (int i = 0; i < 10; i++)
            {
                await notificationService
                    .SendLowInStockNotification($"product{i}", i, i, $"#000000{i}");
            }

            var notificationsToDelete = db!.Notifications
                .Take(4);

            // Act // Assert
            foreach (var notification in notificationsToDelete)
            {
                Assert.True(db.Notifications
                    .Any(x => x.Message == notification.Message && 
                        !x.IsDeleted));

                var response = await notificationService.DeleteNotification(notification.Message);

                Assert.True(response);

                Assert.False(db.Notifications
                    .Any(x => x.Message == notification.Message && 
                        !x.IsDeleted));
            }
        }

        [Fact]
        public async Task DeleteNotification_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.DeleteAsync("/Notifications/something");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteNotification_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.DeleteAsync("/Notifications/something");

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
