namespace NutriBest.Server.Tests.Fixtures
{
    using Moq;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Extensions;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Tests.Fixtures.Base;
    using NutriBest.Server.Features.Notifications;

    public class IdentityFixture
    {
        public IIdentityService IdentityService { get; private set; }
        public IdentityController IdentityController { get; private set; }
        public UserManager<User> UserManager { get; private set; }
        public RoleManager<IdentityRole> RoleManager { get; private set; }
        public Mock<IEmailService> EmailServiceMock { get; private set; }
        public Mock<INotificationService> NotificationServiceMock { get; private set; }

        //public IdentityFixture()
        //{
        //    NotificationServiceMock = new Mock<INotificationService>();
        //    EmailServiceMock = new Mock<IEmailService>();

        //    var appSettings = new ApplicationSettings
        //    {
        //        Secret = "your-secret-key-here"
        //    };

        //    Services.Configure<ApplicationSettings>(opts =>
        //        opts.Secret = appSettings.Secret);

        //    Services.AddTransient(_ => NotificationServiceMock.Object);
        //    Services.AddTransient(_ => EmailServiceMock.Object);

        //    Services.AddIdentity();

        //    Services.AddTransient<IIdentityService, IdentityService>();

        //    ServiceProvider = Services.BuildServiceProvider();

        //    UserManager = ServiceProvider.GetRequiredService<UserManager<User>>();
        //    RoleManager = ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        //    IdentityService = ServiceProvider.GetRequiredService<IIdentityService>();

        //    IdentityController = new IdentityController(IdentityService,
        //        UserManager,
        //        CurrentUserServiceMock.Object,
        //        EmailServiceMock.Object,
        //        NotificationServiceMock.Object);

        //    SeedData(RoleManager);
        //}

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
    }
}