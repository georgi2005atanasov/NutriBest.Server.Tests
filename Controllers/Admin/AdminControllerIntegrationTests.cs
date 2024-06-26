namespace NutriBest.Server.Tests.Controllers.Admin
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using NutriBest.Server.Features.Admin.Models;
    using System.Text.Json;
    using Xunit;

    [Collection("Admin Controller Tests")]
    public class AdminControllerIntegrationTests
    {
        private readonly ClientHelper clientHelper;

        private readonly CustomWebApplicationFactoryFixture fixture;

        private readonly ApplicationSettings appSettings;

        public AdminControllerIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
            using (var scope = fixture.Factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                //_userManager = scopedServices.GetRequiredService<UserManager<User>>();
                var options = scopedServices.GetRequiredService<IOptions<ApplicationSettings>>();
                appSettings = options.Value;
            }
        }

        [Fact]
        public async Task AllUsersEndpoint_ShouldReturnAllUsers()
        {
            var client = await clientHelper.GetAdministratorClientAsync();

            var allUsers = await client.GetAsync("/Admin/AllUsers");
            //
            var data = await allUsers.Content.ReadAsStringAsync();
            var allUsersModel = JsonSerializer.Deserialize<IEnumerable<UserServiceModel>>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new List<UserServiceModel>();

            Assert.Equal(3, allUsersModel.Count());
        }
    }
}
