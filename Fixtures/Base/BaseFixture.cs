namespace NutriBest.Server.Tests.Fixtures.Base
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using NutriBest.Server.Data;
    using NutriBest.Server.Infrastructure.Services;
    using NutriBest.Server.Infrastructure.Extensions;

    public class BaseTestsFixture : IDisposable
    {
        public NutriBestDbContext DbContext { get; protected set; }
        public ServiceProvider ServiceProvider { get; protected set; }
        public Mock<ICurrentUserService> CurrentUserServiceMock { get; private set; }
        public ServiceCollection Services { get; private set; }

        public BaseTestsFixture()
        {
            Services = new ServiceCollection();
            Services.AddDbContext<NutriBestDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

            Services.AddIdentity();

            Services.AddLogging();

            CurrentUserServiceMock = new Mock<ICurrentUserService>();
            Services.AddTransient(_ => CurrentUserServiceMock.Object);

            ServiceProvider = Services.BuildServiceProvider();
            DbContext = ServiceProvider.GetRequiredService<NutriBestDbContext>();
        }

        public void Dispose()
        {
            DbContext.Database.EnsureDeleted();
            ServiceProvider.Dispose();
        }
    }
}