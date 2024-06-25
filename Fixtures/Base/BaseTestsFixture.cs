namespace NutriBest.Server.Tests.Fixtures.Base
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Moq;
    using NutriBest.Server.Data;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Features.Email;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Infrastructure.Services;
    using NutriBest.Server.Infrastructure.Extensions;

    public class BaseTestsFixture : IDisposable
    {
        public NutriBestDbContext DbContext { get; protected set; }
        public ServiceProvider ServiceProvider { get; protected set; }
        public Mock<ICurrentUserService> CurrentUserServiceMock { get; private set; }

        public BaseTestsFixture()
        {
            var services = new ServiceCollection();
            services.AddDbContext<NutriBestDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            services.AddIdentity();

            CurrentUserServiceMock = new Mock<ICurrentUserService>();
            services.AddTransient(_ => CurrentUserServiceMock.Object);

            ServiceProvider = services.BuildServiceProvider();
            DbContext = ServiceProvider.GetRequiredService<NutriBestDbContext>();
        }
        
        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            ServiceProvider.Dispose();
        }
    }
}
