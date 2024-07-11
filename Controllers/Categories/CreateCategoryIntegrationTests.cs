namespace NutriBest.Server.Tests.Controllers.Categories
{
    using Xunit;
    using System.Net;
    using System.Text.Json;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Categories.Models;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Categories Controller Tests")]
    public class CreateCategoryIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CreateCategoryIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task CreateCategory_ShouldBeExecuted_ForAdmin()
        {
            var client = await clientHelper.GetAdministratorClientAsync();

            var categoryModel = new CreateCategoryServiceModel
            {
                Name = "UniqueProducts"
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == "UniqueProducts"));

            var response = await client.PostAsync("/Categories", formData);

            var data = await response.Content.ReadAsStringAsync();

            Assert.Equal("13", data);
            Assert.True(db!.Categories
                .Any(x => x.Name == "UniqueProducts"));
        }

        [Fact]
        public async Task CreateCategory_ShouldBeExecuted_ForEmployee()
        {
            var client = await clientHelper.GetEmployeeClientAsync();

            var categoryModel = new CreateCategoryServiceModel
            {
                Name = "UniqueProducts"
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == "UniqueProducts"));

            var response = await client.PostAsync("/Categories", formData);
            var data = await response.Content.ReadAsStringAsync();

            Assert.Equal("13", data);
            Assert.True(db!.Categories
                .Any(x => x.Name == "UniqueProducts"));
        }

        [Fact]
        public async Task CreateCategory_ShouldReturnBadRequest_ForAlreadyExistingCategory()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var categoryModel = new CreateCategoryServiceModel
            {
                Name = "UniqueProducts"
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == "UniqueProducts"));

            // Act
            await client.PostAsync("/Categories", formData);

            var response = await client.PostAsync("/Categories", formData);

            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // This option allows matching property names ignoring case
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CreateCategory_ShouldReturnUnauthorized_ForAnonymous()
        {
            var client = clientHelper.GetAnonymousClient();

            var categoryModel = new CreateCategoryServiceModel
            {
                Name = "UniqueProducts"
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == "UniqueProducts"));

            var response = await client.PostAsync("/Categories", formData);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(db!.Categories
                .Any(x => x.Name == "UniqueProducts"));
        }

        [Fact]
        public async Task CreateCategory_ShouldReturnForbidden_ForUsers()
        {
            var client = await clientHelper.GetOtherUserClientAsync();

            var categoryModel = new CreateCategoryServiceModel
            {
                Name = "UniqueProducts"
            };

            // Convert the category model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(categoryModel.Name), "Name" }
            };

            Assert.False(db!.Categories
                .Any(x => x.Name == "UniqueProducts"));

            var response = await client.PostAsync("/Categories", formData);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.False(db!.Categories
                .Any(x => x.Name == "UniqueProducts"));
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedCategories();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
