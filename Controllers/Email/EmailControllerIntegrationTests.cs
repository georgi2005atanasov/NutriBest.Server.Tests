using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Email
{
    using Moq;
    using NutriBest.Server.Features.Email.Models;
    using System.Net;
    using System.Net.Http.Json;
    using System.Text.Json;
    using Xunit;
    using static SuccessMessages.EmailController;

    [Collection("Email Controller Tests")]
    public class EmailControllerIntegrationTests : IAsyncLifetime
    {
        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        public EmailControllerIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task SendConfirmOrder_ShouldCallService_AndReturnOK()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailConfirmOrderModel
            {
                CustomerName = "Pesho",
                OrderId = 100,
                ConfirmationUrl = "someUrl",
                To = "gogida9876@abv.bg",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendConfirmOrderEmail", model);

            // Assert
            fixture.Factory.EmailServiceMock!
                .Verify(x => x.SendConfirmOrder(model), Times.Once);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendConfirmOrder_ShouldReturnBadRequest_WhenEmailIsNotPassed()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailConfirmOrderModel
            {
                CustomerName = "Pesho",
                OrderId = 100,
                ConfirmationUrl = "someUrl",
                To = "",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendConfirmOrderEmail", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendConfirmOrder_ShouldHandleExceptions_WithBadRequest()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailConfirmOrderModel
            {
                CustomerName = "Pesho",
                OrderId = 100,
                ConfirmationUrl = "someUrl",
                To = "gogida9876@abv.bg",
                Subject = "subject"
            };

            fixture.Factory.EmailServiceMock!
                .Setup(x => x.SendConfirmOrder(model))
                .Throws<Exception>();

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendConfirmOrderEmail", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendConfirmedOrderToAdmin_ShouldCallService_AndReturnOK()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailConfirmedOrderModel
            {
                OrderDetailsUrl = "someUrl",
                Subject = "Order Confirmation",
                OrderId = 100
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendConfirmedOrderToAdmin", model);

            // Assert
            fixture.Factory.EmailServiceMock!
                .Verify(x => x.SendConfirmedOrderToAdmin(model), Times.Once);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public async Task SendConfirmedOrderToAdmin_ShouldHandleExceptions_WithBadRequest()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailConfirmedOrderModel
            {
                OrderDetailsUrl = "someUrl",
                Subject = "subject",
                OrderId = 100
            };

            fixture.Factory.EmailServiceMock!
                .Setup(x => x.SendConfirmedOrderToAdmin(model))
                .Throws<Exception>();

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendConfirmedOrderToAdmin", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendOrderToAdmin_ShouldCallService_AndReturnOK()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailOrderModel
            {
                CustomerEmail = "someEmail@abv.bg",
                CustomerName = "Some Name",
                PhoneNumber = "",
                TotalPrice = "100",
                OrderDetailsUrl = "someUrl",
                Subject = "subject",
                OrderId = 100
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendOrderToAdmin", model);
            response.EnsureSuccessStatusCode();
            // Assert
            fixture.Factory.EmailServiceMock!
                .Verify(x => x.SendNewOrderToAdmin(model));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendOrderToAdmin_ShouldHandleExceptions_WithBadRequest()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailOrderModel
            {
                CustomerEmail = "someEmail@abv.bg",
                CustomerName = "Some Name",
                PhoneNumber = "",
                TotalPrice = "100",
                OrderDetailsUrl = "someUrl",
                Subject = "subject",
                OrderId = 100
            };

            fixture.Factory.EmailServiceMock!
                .Setup(x => x.SendNewOrderToAdmin(model))
                .Throws<Exception>();

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendOrderToAdmin", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ForgottenPassword_ShouldCallService_AndReturnOK()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailModel
            {
                To = "user@example.com",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/ForgottenPassword", model);

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            bool isSuccess = root.GetProperty("isSuccess").GetBoolean();
            string message = root.GetProperty("message").GetString() ?? "";

            // Assert
            fixture.Factory.EmailServiceMock!
                .Verify(x => x.SendForgottenPassword(It.IsAny<EmailModel>(), It.IsAny<string>()));
            Assert.True(isSuccess);
            Assert.Equal(IfEmailIsValidLinkIsSent, message);
        }

        [Fact]
        public async Task ForgottenPassword_ShouldReturnBadRequest_WhenEmailIsNotPassed()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailModel
            {
                To = "",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/ForgottenPassword", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ForgottenPassword_ShouldReturnOK_WhenUserIsNotFound()
        {
            var client = clientHelper.GetAnonymousClient();

            var model = new EmailModel
            {
                To = "some user",
                Subject = "subject"
            };

            var response = await client.PostAsJsonAsync("/Email/ForgottenPassword", model);

            var jsonString = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            bool isSuccess = root.GetProperty("isSuccess").GetBoolean();
            string message = root.GetProperty("message").GetString() ?? "";

            Assert.True(isSuccess);
            Assert.Equal(IfEmailIsValidLinkIsSent, message);
        }

        [Fact]
        public async Task ForgottenPassword_ShouldHandleExceptions_WithBadRequest()
        {
            var client = clientHelper.GetAnonymousClient();

            var model = new EmailOrderModel
            {
                CustomerEmail = "someEmail@abv.bg",
                CustomerName = "Some Name",
                PhoneNumber = "",
                TotalPrice = "100",
                OrderDetailsUrl = "someUrl",
                Subject = "subject",
                OrderId = 100
            };

            fixture.Factory.EmailServiceMock!
                .Setup(x => x.SendNewOrderToAdmin(model))
                .Throws<Exception>();

            var response = await client.PostAsJsonAsync("/Email/SendOrderToAdmin", model);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendPromoCode_ShouldCallService_AndReturnOK()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new SendPromoEmailModel
            {
                PromoCodeDescription = "Some descripion",
                To = "someEmail@abv.bg",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendPromoCode", model);

            // Assert
            fixture.Factory.EmailServiceMock!
                .Verify(x => x.SendPromoCode(model));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendPromoCode_ShouldReturnBadRequest_WhenEmailIsNotPassed()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new SendPromoEmailModel
            {
                PromoCodeDescription = "Some descripion",
                To = "",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendPromoCode", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendPromoCode_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new SendPromoEmailModel
            {
                PromoCodeDescription = "Some descripion",
                To = "",
                Subject = "subject"
            };

            fixture.Factory.EmailServiceMock!
                .Setup(x => x.SendPromoCode(model))
                .Throws<Exception>();

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendPromoCode?groupType=all", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendPromoCode_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            var model = new SendPromoEmailModel
            {
                PromoCodeDescription = "Some descripion",
                To = "someEmail@example.com",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendPromoCode", model);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SendPromoCode_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new SendPromoEmailModel
            {
                PromoCodeDescription = "Some descripion",
                To = "someEmail@example.com",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendPromoCode", model);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SendJoinedToNewsletter_ShouldCallService_AndReturnOK()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new EmailModel
            {
                To = "user@example.com",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendJoinedToNewsletter", model);

            // Assert
            fixture.Factory.EmailServiceMock!
                .Verify(x => x.SendJoinedToNewsletter(model));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendJoinedToNewsletter_ShouldReturnBadRequest_WhenEmailIsNotPassed()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new EmailModel
            {
                To = "",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendJoinedToNewsletter", model);

            // Assert

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendJoinedToNewsletter_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new EmailModel
            {
                To = "",
                Subject = "subject"
            };
            fixture.Factory.EmailServiceMock!
                .Setup(x => x.SendJoinedToNewsletter(model))
                .Throws<Exception>();

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendJoinedToNewsletter", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendMessageToSubscribers_ShouldCallService_AndReturnOK()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new EmailSubscribersServiceModel
            {
                Body = "some topic",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendMessageToSubscribers?groupType=all", model);

            // Assert
            fixture.Factory.EmailServiceMock!
                .Verify(x => x.SendMessageToSubscribers(model, "all"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendMessageToSubscribers_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new EmailSubscribersServiceModel
            {
                Body = "some topic",
                Subject = "subject"
            };
            fixture.Factory.EmailServiceMock!
                .Setup(x => x.SendMessageToSubscribers(model, "all"))
                .Throws<Exception>();

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendMessageToSubscribers?groupType=all", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendMessageToSubscribers_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            var model = new EmailSubscribersServiceModel
            {
                Body = "some topic",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendMessageToSubscribers?groupType=all", model);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SendMessageToSubscribers_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailSubscribersServiceModel
            {
                Body = "some topic",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendMessageToSubscribers?groupType=all", model);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task SendPromoCodesToSubscribers_ShouldCallService_AndReturnOK()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new EmailSubscribersPromoCodeServiceModel
            {
                PromoCodeDescription = "Description",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendPromoCodesToSubscribers?groupType=all", model);

            // Assert
            fixture.Factory.EmailServiceMock!
                .Verify(x => x.SendPromoCodesToSubscribers(model, "all"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task SendPromoCodesToSubscribers_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var model = new EmailSubscribersPromoCodeServiceModel
            {
                PromoCodeDescription = "Description",
                Subject = "subject"
            };

            fixture.Factory.EmailServiceMock!
                .Setup(x => x.SendPromoCodesToSubscribers(model, "all"))
                .Throws<Exception>();

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendPromoCodesToSubscribers?groupType=all", model);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendPromoCodesToSubscribers_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            var model = new EmailSubscribersPromoCodeServiceModel
            {
                PromoCodeDescription = "Description",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendPromoCodesToSubscribers?groupType=all", model);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task SendPromoCodesToSubscribers_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            var model = new EmailSubscribersServiceModel
            {
                Body = "some topic",
                Subject = "subject"
            };

            // Act
            var response = await client.PostAsJsonAsync("/Email/SendPromoCodesToSubscribers?groupType=all", model);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            fixture.Factory.EmailServiceMock!.Reset();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
