namespace NutriBest.Server.Tests.Fixtures
{
    using Moq;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Extensions;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Tests.Fixtures.Base;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Features.Newsletter;

    public class IdentityFixture : BaseFixture
    {
        public IIdentityService IdentityService { get; private set; }

        public IdentityController IdentityController { get; private set; }

        public INewsletterService NewsletterService { get; private set; }

        public Mock<IEmailService> EmailServiceMock { get; private set; }

        public Mock<INotificationService> NotificationServiceMock { get; private set; }

        public IdentityFixture() 
            : base()
        {
            NotificationServiceMock = new Mock<INotificationService>();
            EmailServiceMock = new Mock<IEmailService>();
            ApplicationSettings = new Mock<IOptions<ApplicationSettings>>();

            Services.AddTransient(_ => NotificationServiceMock.Object);
            Services.AddTransient(_ => EmailServiceMock.Object);
            Services.AddTransient(_ => IdentityService!);

            Services.AddIdentity();

            ServiceProvider = Services.BuildServiceProvider();
        }
    }
}