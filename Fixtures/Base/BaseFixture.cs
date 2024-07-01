namespace NutriBest.Server.Tests.Fixtures.Base
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NutriBest.Server.Data;
    using NutriBest.Server.Infrastructure.Services;
    using NutriBest.Server.Infrastructure.Extensions;
    using Microsoft.Extensions.Options;
    using Microsoft.AspNetCore.Identity;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Features.Newsletter;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Features.Identity;

    public class BaseFixture
    {
        public NutriBestDbContext? DbContext { get; private set; }

        public ServiceProvider? ServiceProvider { get; protected set; }

        public UserManager<User>? UserManager { get; private set; }

        public RoleManager<IdentityRole>? RoleManager { get; private set; }

        public IIdentityService? IdentityService { get; private set; }

        public IdentityController? IdentityController { get; private set; }

        public INewsletterService? NewsletterService { get; private set; }

        public Mock<IEmailService>? EmailServiceMock { get; private set; }

        public Mock<INotificationService>? NotificationServiceMock { get; private set; }

        public Mock<IOptions<ApplicationSettings>>? ApplicationSettings { get; set; }

        public Mock<ICurrentUserService> CurrentUserServiceMock { get; private set; } = new Mock<ICurrentUserService>();

        public ServiceCollection Services { get; private set; }

        public BaseFixture()
        {
            Services = new ServiceCollection();
            Services.AddDbContext<NutriBestDbContext>(options =>
                options.UseInMemoryDatabase("BaseFixtureDb"));

            InitializeServices();
        }

        public virtual void InitializeServices()
        {
            DbContext = CreateDbContext();

            var appSettings = new ApplicationSettings
            {
                Secret = "your-secret-key-here"
            };

            Services.Configure<ApplicationSettings>(opts =>
                opts.Secret = appSettings.Secret);

            Services.AddIdentity();

            Services.AddLogging();

            CurrentUserServiceMock = new Mock<ICurrentUserService>();
            NotificationServiceMock = new Mock<INotificationService>();
            EmailServiceMock = new Mock<IEmailService>();
            ApplicationSettings = new Mock<IOptions<ApplicationSettings>>();

            Services.AddTransient(_ => CurrentUserServiceMock.Object);
            Services.AddTransient(_ => NotificationServiceMock.Object);
            Services.AddTransient(_ => EmailServiceMock.Object);
            Services.AddTransient(_ => IdentityService!);

            Services.AddIdentity();

            ServiceProvider = Services.BuildServiceProvider();

            UserManager = ServiceProvider.GetRequiredService<UserManager<User>>();
            RoleManager = ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            IdentityService = new IdentityService(DbContext,
                UserManager,
                ApplicationSettings.Object,
                RoleManager,
                null!);

            NewsletterService = new NewsletterService(DbContext,
                UserManager,
                EmailServiceMock.Object,
                NotificationServiceMock.Object);

            IdentityController = new IdentityController(IdentityService,
                UserManager,
                null!,
                EmailServiceMock.Object,
                NotificationServiceMock.Object);

            SeedData(RoleManager);
        }

        protected void SeedData(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Administrator", "Employee", "User" };
            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    roleManager.CreateAsync(new IdentityRole(role)).Wait();
                }
            }
        }

        protected NutriBestDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<NutriBestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Use an in-memory database
                .Options;
            return new NutriBestDbContext(options, CurrentUserServiceMock.Object);
        }
    }
}