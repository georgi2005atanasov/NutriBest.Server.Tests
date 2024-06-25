namespace NutriBest.Server.Tests.Fixtures
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Features.Admin;
    using NutriBest.Server.Tests.Fixtures.Base;
    using Infrastructure.Extensions;

    public class AdminTestsFixture : IdentityTestsFixture
    {
        public IAdminService AdminService { get; set; }
        public AdminController AdminController { get; private set; }

        public AdminTestsFixture()
            : base()
        {
            var appSettings = new ApplicationSettings
            {
                Secret = "your-secret-key-here"
            };

            Services.Configure<ApplicationSettings>(opts =>
                opts.Secret = appSettings.Secret);

            Services.AddTransient<IAdminService, AdminService>(provider =>
            {
                return new AdminService(DbContext, UserManager);
            });

            ServiceProvider = Services.BuildServiceProvider();

            AdminService = ServiceProvider.GetRequiredService<IAdminService>();

            AdminController = new AdminController(DbContext,
                AdminService,
                UserManager,
                RoleManager);
        }
    }
}
