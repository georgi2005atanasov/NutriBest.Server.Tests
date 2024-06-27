namespace NutriBest.Server.Tests
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Data.Models;
    using System;
    using System.Threading.Tasks;
    using Moq;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Features.Email;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        public Mock<INotificationService>? NotificationServiceMock { get; private set; }

        public Mock<IEmailService>? EmailServiceMock { get; private set; }

        public Mock<UserManager<User>>? UserManagerMock { get; set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                NotificationServiceMock = new Mock<INotificationService>();
                EmailServiceMock = new Mock<IEmailService>();

                services.AddTransient(_ => NotificationServiceMock.Object);
                services.AddTransient(_ => EmailServiceMock.Object);

                services.RemoveAll<DbContextOptions<NutriBestDbContext>>();
                services.AddDbContext<NutriBestDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDb");
                });

                var serviceProvider = services.BuildServiceProvider();

                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<NutriBestDbContext>();
                    var userManager = scopedServices.GetRequiredService<UserManager<User>>();
                    var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();

                    // Seed the database with users and roles
                    SeedDatabase(userManager, roleManager);
                }
            });
        }

        public void SeedDatabase(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                new IdentityRole { Name = "Employee", NormalizedName= "EMPLOYEE" },
                new IdentityRole { Name = "User", NormalizedName = "USER" },
                new IdentityRole { Name = "Administrator", NormalizedName = "ADMINISTRATOR" }
            };

            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role.Name).GetAwaiter().GetResult())
                {
                    roleManager.CreateAsync(role).GetAwaiter().GetResult();
                }
            }

            if (userManager.FindByNameAsync("admin").GetAwaiter().GetResult() == null)
            {
                var adminUser = new User { UserName = "admin", Email = "admin@example.com" };
                var employeeUser = new User { UserName = "employee", Email = "employee@example.com" };
                var otherUser = new User { UserName = "user", Email = "user@example.com" };

                CreateUserWithRole(userManager, adminUser, "Password123!", "Administrator").GetAwaiter().GetResult();
                CreateUserWithRole(userManager, employeeUser, "Password123!", "Employee").GetAwaiter().GetResult();
                CreateUserWithRole(userManager, otherUser, "Password123!", "User").GetAwaiter().GetResult();
            }

        }

        private async Task CreateUserWithRole(UserManager<User> userManager, User user, string password, string role)
        {
            var userExists = await userManager.FindByNameAsync(user.UserName);
            if (userExists == null)
            {
                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    throw new Exception($"Error creating user {user.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        public async Task ResetDatabaseAsync()
        {
            using var scope = Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<NutriBestDbContext>();
            var userManager = scopedServices.GetRequiredService<UserManager<User>>();
            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            SeedDatabase(userManager, roleManager);
        }
    }
}