using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Orders
{
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Orders.Models;
    using NutriBest.Server.Infrastructure.Extensions;
    using static ErrorMessages.OrdersController;
    using System.Net.Http.Json;
    using System.Net;
    using NutriBest.Server.Shared.Responses;
    using System.Text.Json;

    [Collection("Orders Controller Tests")]
    public class DeleteOrderIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public DeleteOrderIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task DeleteUserOrder_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "user@example.com",
                "user",
                "TEST USER!!!");
            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);

            Assert.NotEmpty(db!.UsersOrders.Where(x => !x.IsDeleted));

            // Act
            var response = await client.DeleteAsync("/Orders/Admin/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.Orders.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task DeleteUserOrder_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "user@example.com",
                "user",
                "TEST USER!!!");
            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);

            Assert.NotEmpty(db!.UsersOrders.Where(x => !x.IsDeleted));

            // Act
            var response = await client.DeleteAsync("/Orders/Admin/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.Orders.Where(x => !x.IsDeleted));
            Assert.Empty(db!.UsersOrders.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task DeleteGuestOrder_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            await SeedingHelper.SeedGuestOrder(clientHelper);

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);

            Assert.NotEmpty(db!.GuestsOrders.Where(x => !x.IsDeleted));

            // Act
            var response = await client.DeleteAsync("/Orders/Admin/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.GuestsOrders.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task DeleteGuestOrder_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedGuestOrder(clientHelper);

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);

            Assert.NotEmpty(db!.GuestsOrders.Where(x => !x.IsDeleted));

            // Act
            var response = await client.DeleteAsync("/Orders/Admin/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.Orders.Where(x => !x.IsDeleted));
            Assert.Empty(db!.GuestsOrders.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task DeleteOrderWithInvoice_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            await SeedingHelper.SeedUserOrder(clientHelper, 
                true,
                "user@example.com",
                "user",
                "TEST USER!!!");

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            await client.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);

            Assert.NotEmpty(db!.UsersOrders.Where(x => !x.IsDeleted));
            Assert.NotEmpty(db!.Invoices.Where(x => !x.IsDeleted));

            // Act
            var response = await client.DeleteAsync("/Orders/Admin/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Empty(db!.Orders.Where(x => !x.IsDeleted));
            Assert.Empty(db!.Invoices.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnBadRequest_WhenOrderIsInvalid()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedUserOrder(clientHelper, 
                false,
                "user@example.com",
                "user",
                "TEST USER!!!");

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            // Act
            var response = await client.DeleteAsync("/Orders/Admin/10");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(OrderCouldNotBeDeleted, result.Message);
            Assert.NotEmpty(db!.Orders.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnBadRequest_WhenOrderIsNotFinished()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            await SeedingHelper.SeedUserOrder(clientHelper, 
                false,
                "user@example.com",
                "user",
                "TEST USER!!!");

            // Act
            var response = await client.DeleteAsync("/Orders/Admin/1");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(OrderMustBeFinishedBeforeDeletingIt, result.Message);
            Assert.NotEmpty(db!.Orders.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            await SeedingHelper.SeedUserOrder(clientHelper, 
                false,
                "user@example.com",
                "user",
                "TEST USER!!!");

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            var admin = await clientHelper.GetAdministratorClientAsync();
            await admin.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);

            // Act
            var response = await client.DeleteAsync("/Orders/Admin/1");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEmpty(db!.Orders.Where(x => !x.IsDeleted));
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            await SeedingHelper.SeedUserOrder(clientHelper, 
                false,
                "user@example.com",
                "user",
                "TEST USER!!!");

            var statusesModel = new UpdateOrderServiceModel
            {
                IsConfirmed = true,
                IsPaid = true,
                IsShipped = true,
                IsFinished = true
            };

            var admin = await clientHelper.GetAdministratorClientAsync();
            await admin.PutAsJsonAsync("/Orders/ChangeStatus/1", statusesModel);

            // Act
            var response = await client.DeleteAsync("/Orders/Admin/1");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.NotEmpty(db!.Orders.Where(x => !x.IsDeleted));
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
