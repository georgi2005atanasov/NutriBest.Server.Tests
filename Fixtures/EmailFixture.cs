namespace NutriBest.Server.Tests.Fixtures
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Features.PromoCodes;
    using NutriBest.Server.Infrastructure.Services;

    public class EmailFixture : IDisposable
    {
        public NutriBestDbContext DbContext { get; private set; }

        public IConfiguration Configuration { get; private set; }
        
        public IPromoCodeService PromoCodeService { get; private set; }

        public Mock<INotificationService> NotificationServiceMock { get; private set; }
        
        public IEmailService EmailService { get; private set; }

        public EmailFixture()
        {
            var options = new DbContextOptionsBuilder<NutriBestDbContext>()
                .UseInMemoryDatabase(databaseName: "EmailDatabase")
                .Options;

            var currentUserService = new CurrentUserService(null!);

            DbContext = new NutriBestDbContext(options, currentUserService);

            var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Tests.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            NotificationServiceMock = new Mock<INotificationService>(); 
            PromoCodeService = new PromoCodeService(DbContext);
            EmailService = new EmailService(DbContext, Configuration, PromoCodeService, NotificationServiceMock.Object);
        }

        public void Dispose()
        {
            DbContext.Dispose();
        }
    }
}
