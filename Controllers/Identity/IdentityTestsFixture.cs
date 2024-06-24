
namespace NutriBest.Server.Tests.Controllers.Identity
{
    using Moq;
    using Microsoft.AspNetCore.Identity;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Infrastructure.Services;
    using NutriBest.Server.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Extensions;
    using Microsoft.Extensions.Options;

    public class IdentityTestsFixture : IDisposable
    {
        public NutriBestDbContext DbContext { get; private set; }
        public ServiceProvider ServiceProvider { get; private set; }
        public IdentityController IdentityController { get; private set; }
        public UserManager<User> UserManager { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public Mock<IEmailService> EmailServiceMock { get; private set; }
        public Mock<INotificationService> NotificationServiceMock { get; private set; }
        public Mock<ICurrentUserService> CurrentUserServiceMock { get; private set; }

        public IdentityTestsFixture()
        {
            var services = new ServiceCollection();
            services.AddDbContext<NutriBestDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddIdentity();

            services.AddLogging();

            NotificationServiceMock = new Mock<INotificationService>();
            EmailServiceMock = new Mock<IEmailService>();
            CurrentUserServiceMock = new Mock<ICurrentUserService>();

            var appSettings = new ApplicationSettings
            {
                Secret = "your-secret-key-here"
            };
            services.Configure<ApplicationSettings>(opts => opts.Secret = appSettings.Secret);

            services.AddTransient(_ => NotificationServiceMock.Object);
            services.AddTransient(_ => EmailServiceMock.Object);
            services.AddTransient(_ => CurrentUserServiceMock.Object);
            services.AddTransient<IIdentityService, IdentityService>(provider =>
            {
                var userManager = provider.GetRequiredService<UserManager<User>>();
                var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
                var dbContext = provider.GetRequiredService<NutriBestDbContext>();
                var options = provider.GetRequiredService<IOptions<ApplicationSettings>>();
                return new IdentityService(userManager, options, roleManager, dbContext);
            });

            ServiceProvider = services.BuildServiceProvider();

            DbContext = ServiceProvider.GetRequiredService<NutriBestDbContext>();
            UserManager = ServiceProvider.GetRequiredService<UserManager<User>>();
            RoleManager = ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            SeedData(RoleManager);

            var identityService = ServiceProvider.GetRequiredService<IIdentityService>();

            IdentityController = new IdentityController(
                identityService,
                UserManager,
                CurrentUserServiceMock.Object,
                EmailServiceMock.Object,
                NotificationServiceMock.Object
            );
        }

        private void SeedData(RoleManager<IdentityRole> roleManager)
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

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            ServiceProvider.Dispose();
        }
    }
}
