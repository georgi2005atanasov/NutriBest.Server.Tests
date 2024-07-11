namespace NutriBest.Server.Tests.InfrastructureTests.ServicesTests
{
    using System.Security.Claims;
    using Xunit;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Infrastructure.Services;

    [Collection("Infrastructure Tests")]
    public class CurrentUserServiceTests : IAsyncLifetime
    {
        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        private IHttpContextAccessor? httpContextAccessor;

        public CurrentUserServiceTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public void CurrentUserServiceShouldReturnUserClaims_Correctly()
        {
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
                new Claim(ClaimTypes.Name, "Test User"),
            };

            // Set up HttpContext
            httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "TestAuthentication"))
                }
            };

            var currentUserService = new CurrentUserService(httpContextAccessor);

            var userName = currentUserService.GetUserName();
            var id = currentUserService.GetUserId();

            Assert.Equal("Test User", userName);
            Assert.Equal("test-user-id", id);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            httpContextAccessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
