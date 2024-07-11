namespace NutriBest.Server.Tests.Fixtures
{
    using Microsoft.Extensions.Configuration;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.PromoCodes;
    using NutriBest.Server.Tests.Fixtures.Base;

    public class EmailFixture : BaseFixture
    {
        public IConfiguration Configuration { get; private set; }

        public IPromoCodeService? PromoCodeService { get; private set; }

        public IEmailService? EmailService { get; private set; }

        public EmailFixture()
            : base()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Tests.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            InitializeServices();
        }

        public override void InitializeServices()
        {
            base.InitializeServices();
            PromoCodeService = new PromoCodeService(DbContext!);
            EmailService = new EmailService(DbContext!,
                Configuration,
                PromoCodeService,
                NotificationServiceMock!.Object);
        }
    }
}
