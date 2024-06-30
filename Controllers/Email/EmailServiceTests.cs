namespace NutriBest.Server.Tests.Controllers.Email
{
    using NutriBest.Server.Data;
    using NutriBest.Server.Tests.Fixtures;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.Email.Models;
    using Xunit;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Features.PromoCodes;
    using Moq;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Features.Identity;

    public class EmailServiceTests : IClassFixture<EmailFixture>, IClassFixture<IdentityFixture>
    {
        private readonly NutriBestDbContext db;
        private readonly IEmailService emailService;
        private readonly IPromoCodeService promoCodeService;
        private readonly IIdentityService identityService;
        private readonly Mock<INotificationService> notificationServiceMock;


        public EmailServiceTests(EmailFixture fixture, IdentityFixture identityFixture)
        {
            this.db = fixture.DbContext;
            this.emailService = fixture.EmailService;
            this.promoCodeService = fixture.PromoCodeService;
            this.notificationServiceMock = fixture.NotificationServiceMock;
            this.identityService = identityFixture.IdentityService;
        }

        [Fact]
        public async Task SendPromoCode_ShouldThrowNullException()
        {
            // Arrange
            var emailModel = new SendPromoEmailModel
            {
                PromoCodeDescription = "Some descripion",
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
            await promoCodeService.Create(1, 10, "20% OFF!");
            var emailModel = new SendPromoEmailModel
            {
                PromoCodeDescription = "20% OFF!",
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
            // Arrange
            var emailModel = new EmailSubscribersPromoCodeServiceModel
            {
                PromoCodeDescription = "Description",
                Subject = "subject"
            };

            // Act
            // await emailService.SendPromoCodesToSubscribers(emailModel, groupType: "all");
        }
    }
}
