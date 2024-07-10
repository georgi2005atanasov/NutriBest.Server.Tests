namespace NutriBest.Server.Tests.Controllers.Notifications
{
    using Xunit;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using System.Net;
    using System.Text.Json;
    using AutoMapper;
    using Moq;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Infrastructure.Extensions;
    using NutriBest.Server.Features.Notifications.Hubs;
    using NutriBest.Server.Features.Notifications.Models;
    using NutriBest.Server.Features.Notifications.Mappings;

    [Collection("Notifications Controller Tests")]
    public class AllNotificationsIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        private IMapper? mapper;

        public AllNotificationsIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllNotificationsEndpoint_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

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
                mapper!);

            for (int i = 0; i < 10; i++)
            {
                await notificationService
                    .SendLowInStockNotification($"product{i}", i, i, $"#000000{i}");
            }

            // Act
            var result = await notificationService.All(1);

            // Assert
            Assert.Equal(10, result.TotalNotifications);
            Assert.Equal(10, result.Notifications.Count);
        }

        [Fact]
        public async Task AllNotificationsEndpoint_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

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
                mapper!);

            for (int i = 0; i < 10; i++)
            {
                await notificationService
                    .SendLowInStockNotification($"product{i}", i, i + 5, $"#000000{i}");
            }

            // Act
            var result = await notificationService.All(1);

            // Assert
            Assert.Equal(10, result.TotalNotifications);
            Assert.Equal(10, result.Notifications.Count);
            Assert.Equal(5, result.Notifications
                        .Where(x => x.Priority == "Low")
                        .Count());
            Assert.Equal(5, result.Notifications
                       .Where(x => x.Priority == "Medium")
                       .Count());
        }

        [Fact]
        public async Task AllNotificationsEndpoint_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            // Act
            var response = await client.GetAsync("/Notifications?page=1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("", data);
        }

        [Fact]
        public async Task AllNotificationsEndpoint_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            // Act
            var response = await client.GetAsync("/Notifications?page=1");
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
            mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
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
