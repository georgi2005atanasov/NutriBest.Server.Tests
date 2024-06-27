namespace NutriBest.Server.Tests.Controllers.Identity
{
    using Xunit;
    using System.Text.Json;
    using System.Net.Http.Json;
    using NutriBest.Server.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using NutriBest.Server.Shared.Responses;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Features.Identity.Models;

    [Collection("Identity Controller Tests")]
    public class RolesIntegrationTests
    {
        private ClientHelper clientHelper;

        public RolesIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
        }

        [Fact]
        public async Task RolesEndpoint_ShouldReturnRolesServiceModel()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Identity/Roles");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var rolesResult = JsonSerializer
                .Deserialize<RolesServiceModel>(data, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new RolesServiceModel();

            Assert.Equal(2, rolesResult.Roles.Count);
            Assert.DoesNotContain("Administrator", rolesResult.Roles);
        }
    }
}
