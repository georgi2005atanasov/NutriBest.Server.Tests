namespace NutriBest.Server.Tests.Controllers.Email
{
    using Xunit;
    using Moq;
    using NutriBest.Server.Data;
    using NutriBest.Server.Tests.Fixtures;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.Email.Models;
    using NutriBest.Server.Features.PromoCodes;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Newsletter;

    public class EmailServiceTests : IClassFixture<EmailFixture>, IDisposable
    {
        private EmailFixture fixture;
        private NutriBestDbContext db;
        private IEmailService emailService;
        private IPromoCodeService promoCodeService;
        private IIdentityService identityService;
        private Mock<INotificationService> notificationServiceMock;
        private INewsletterService newsletterService;

        public EmailServiceTests(EmailFixture fixture)
        {
            this.fixture = fixture;
            this.db = fixture.DbContext;
            this.emailService = fixture.EmailService;
            this.promoCodeService = fixture.PromoCodeService;
            this.notificationServiceMock = fixture.NotificationServiceMock;
            this.identityService = fixture.IdentityService;
            this.newsletterService = fixture.NewsletterService;
        }

        public void Dispose()
        {
            db.Dispose();
            InitializeTestServices();
        }

        internal void InitializeTestServices()
        {
            fixture.InitializeServices();
            this.db = fixture.DbContext;
            this.emailService = fixture.EmailService;
            this.promoCodeService = fixture.PromoCodeService;
            this.notificationServiceMock = fixture.NotificationServiceMock;
            this.identityService = fixture.IdentityService;
            this.newsletterService = fixture.NewsletterService;
        }

        [Fact]
        public async Task SendConfirmOrder_ShouldBeExecuted()
        {
            // Arrange
            var emailModel = new EmailConfirmOrderModel
            {
                OrderId = 100,
                ConfirmationUrl = "someUrl",
                CustomerName = "Pesho",
                To = "gogida9876@abv.bg",
                Subject = "subject"
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await emailService.SendConfirmOrder(emailModel);
            });

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendNewOrderToAdmin_ShouldBeExecuted()
        {
            // Arrange
            var emailModel = new EmailOrderModel
            {
                OrderId = 100,
                CustomerName = "Pesho",
                Subject = "subject",
                OrderDetailsUrl = "someUrl",
                CustomerEmail = "gogida9876@abv.bg",
                PhoneNumber = "",
                TotalPrice = "100"
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await emailService.SendNewOrderToAdmin(emailModel);
            });

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendForgottenPassword_ShouldBeExecuted()
        {
            // Arrange
            var emailModel = new EmailModel
            {
                Subject = "subject",
                To = "gogida9876@abv.bg"
            };
            var groupType = "all";

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await emailService.SendForgottenPassword(emailModel, groupType);
            });

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendPromoCode_ShouldThrowNullException()
        {
            // Arrange
            var emailModel = new SendPromoEmailModel
            {
                PromoCodeDescription = "Test Promo Code Description!",
                To = "someEmail@abv.bg",
                Subject = "subject"
            };

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await emailService.SendPromoCode(emailModel));
        }

        [Fact]
        public async Task SendPromoCode_ShouldBeExecuted()
        {
            // Arrange
            await promoCodeService.Create(1, 10, "Test Promo Code Description!");
            var emailModel = new SendPromoEmailModel
            {
                PromoCodeDescription = "Test Promo Code Description!",
                To = "gogida9876@abv.bg",
                Subject = "subject"
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await emailService.SendPromoCode(emailModel);
            });

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendConfirmedOrderToAdmin_ShouldBeExecuted()
        {
            // Arrange
            var emailModel = new EmailConfirmedOrderModel
            {
                OrderId = 100,
                Subject = "subject",
                OrderDetailsUrl = "someUrl",
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await emailService.SendConfirmedOrderToAdmin(emailModel);
            });

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendJoinedToNewsletter_ShouldBeExecuted()
        {
            // Arrange
            var emailModel = new EmailModel
            {
                Subject = "subject",
                To = "gogida9876@abv.bg"
            };

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await emailService.SendJoinedToNewsletter(emailModel);
            });

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task SendMessageToSubscribers_ShouldSendNotification()
        {
            // Arrange
            var emailModel = new EmailSubscribersServiceModel
            {
                Body = "some topic",
                Subject = "subject"
            };

            // Act
            await emailService.SendMessageToSubscribers(emailModel, groupType: "all");

            // Assert
            notificationServiceMock
                .Verify(x => x
                        .SendNotificationToAdmin("success",
                        "Successfully Sent Message to the Newsletter Subscribers!"));
        }

        [Fact]
        public async Task SendPromoCodesToSubscribers_ShouldThrowException_WhenPromoCodesAreNotEnough()
        {
            // Arrange/Act
            var emailModel = new EmailSubscribersPromoCodeServiceModel
            {
                PromoCodeDescription = "Test Promo Code Description!",
                Subject = "subject"
            };

            await identityService.CreateUser("emailServiceUser",
                "emailServiceUser@example.com",
                "Pesho12345");
            await identityService.CreateUser("emailServiceUser2",
                "emailServiceUser2@example.com",
                "Pesho12345");

            await newsletterService.Add("emailServiceUser@example.com");
            await newsletterService.Add("emailServiceUser2@example.com");

            await promoCodeService.Create(20, 1, "Test Promo Code Description!");

            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await emailService.SendPromoCodesToSubscribers(emailModel, groupType: "all"));
        }

        [Fact]
        public async Task SendPromoCodesToSubscribers_ShouldBeExecuted()
        {
            // Arrange
            var emailModel = new EmailSubscribersPromoCodeServiceModel
            {
                PromoCodeDescription = "Test Promo Code Description!",
                Subject = "subject"
            };
            await promoCodeService.Create(20, 2, "Test Promo Code Description!");

            await identityService.CreateUser("emailServiceUser10",
                "emailServiceUser10@example.com",
                "Pesho12345");
            await identityService.CreateUser("emailServiceUser20",
                "emailServiceUser20@example.com",
                "Pesho12345");

            await newsletterService.Add("emailServiceUser10@example.com");
            await newsletterService.Add("emailServiceUser20@example.com");

            // Act
            var exception = await Record.ExceptionAsync(async () =>
            {
                await emailService.SendPromoCodesToSubscribers(emailModel, groupType: "all");
            });
        }
    }
}
