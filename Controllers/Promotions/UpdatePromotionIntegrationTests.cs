using NutriBest.Server.Utilities.Messages;

namespace NutriBest.Server.Tests.Controllers.Promotions
{
    using System.Net;
    using System.Text.Json;
    using Xunit;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Shared.Responses;
    using NutriBest.Server.Features.Promotions.Models;
    using Infrastructure.Extensions;
    using static ErrorMessages.PromotionsController;
    using static ErrorMessages.BrandsController;
    using Microsoft.EntityFrameworkCore;

    [Collection("Promotions Controller Tests")]
    public class UpdatePromotionIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public UpdatePromotionIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task UpdatePromotion_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();

            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            var updateModel = new UpdatePromotionServiceModel
            {
                Brand = "Muscle Tech",
                DiscountPercentage = "30",
                Description = "UPDATED PROMOTION!!!",
                Category = "Proteins"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.Category), "Category" },
            };

            // Act
            var response = await client.PutAsync("/Promotions/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);
            var promotionToTest = await db!.Promotions.FirstAsync();
            Assert.Equal("Muscle Tech", promotionToTest.Brand);
            Assert.Equal("UPDATED PROMOTION!!!", promotionToTest.Description);
            Assert.Equal("Proteins", promotionToTest.Category);
            Assert.Equal("30", $"{promotionToTest.DiscountPercentage}");
        }

        public async Task UpdatePromotion_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();

            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            var updateModel = new UpdatePromotionServiceModel
            {
                Brand = "Muscle Tech",
                DiscountPercentage = "30",
                Description = "UPDATED PROMOTION!!!",
                Category = "Proteins"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.Category), "Category" },
            };

            // Act
            var response = await client.PutAsync("/Promotions/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("true", data);
        }

        [Fact]
        public async Task UpdatePromotion_ShouldBeExecuted_AndShouldAlsoBeApplied()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();
            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            var updateModel = new UpdatePromotionServiceModel
            {
                Brand = "Muscle Tech",
                DiscountPercentage = "30",
                Description = "UPDATED PROMOTION!!!",
                Category = "Proteins"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.Category), "Category" },
            };

            // Act
            var response = await client.PutAsync("/Promotions/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(db!.Products
                        .Where(x => x.PromotionId == 1)
                        .Count() == 2);
            Assert.Equal("true", data);
        }

        [Fact]
        public async Task UpdatePromotion_ShouldReturnBadRequest_BecauseDescriptionIsAlreadyTaken()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var (formDataPercentDiscount, formDataAmountDiscount) = SeedingHelper.GetTwoPromotions();

            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            await client.PostAsync("/Promotions", formDataAmountDiscount);
            await client.PutAsync("/Promotions/Status/2", null);

            var updateModel = new UpdatePromotionServiceModel
            {
                Brand = "Muscle Tech",
                DiscountPercentage = "30",
                Description = "TEST PROMO2",
                Category = "Proteins"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.Category), "Category" },
            };

            // Act
            var response = await client.PutAsync("/Promotions/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(PromotionAlreadyExists, result.Message);
        }

        [Fact]
        public async Task UpdatePromotion_ShouldReturnBadRequest_BecausePromotionIsNull()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();

            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            var updateModel = new UpdatePromotionServiceModel
            {
                Brand = "Muscle Tech",
                DiscountPercentage = "30",
                Description = "UPDATED PROMOTION!!!",
                Category = "Proteins"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.Category), "Category" },
            };

            // Act
            var response = await client.PutAsync("/Promotions/1678", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(PromotionDoesNotExists, result.Message);
        }

        [Fact]
        public async Task UpdatePromotion_ShouldReturnBadRequest_BecauseDiscountAmountIsTooBig()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            var updateModel = new UpdatePromotionServiceModel
            {
                Brand = "Muscle Tech",
                DiscountAmount = "3000",
                Description = "UPDATED PROMOTION!!!",
                Category = "Proteins"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.DiscountAmount), "DiscountAmount" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.Category), "Category" },
            };

            // Act
            var response = await client.PutAsync("/Promotions/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(NewDiscountCannotBeApplied, result.Message);
        }

        [Fact]
        public async Task UpdatePromotion_ShouldReturnBadRequest_WhenBrandIsInvalid()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();
            var (formDataPercentDiscount, _) = SeedingHelper.GetTwoPromotions();

            await SeedingHelper.SeedSevenProducts(clientHelper);
            await client.PostAsync("/Promotions", formDataPercentDiscount);
            await client.PutAsync("/Promotions/Status/1", null);

            var updateModel = new UpdatePromotionServiceModel
            {
                Brand = "INvalidBrand123",
                DiscountPercentage = "30",
                Description = "UPDATED PROMOTION!!!",
                Category = "Proteins"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.Category), "Category" },
            };

            // Act
            var response = await client.PutAsync("/Promotions/1", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(InvalidBrandName, result.Message);
        }

        [Fact]
        public async Task UpdatePromotion_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();
            
            var updateModel = new UpdatePromotionServiceModel
            {
                Brand = "Muscle Tech",
                DiscountPercentage = "30",
                Description = "UPDATED PROMOTION!!!",
                Category = "Proteins"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.Category), "Category" },
            };

            // Act
            var response = await client.PutAsync("/Promotions/1", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task UpdatePromotion_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();
            
            var updateModel = new UpdatePromotionServiceModel
            {
                Brand = "Muscle Tech",
                DiscountPercentage = "30",
                Description = "UPDATED PROMOTION!!!",
                Category = "Proteins"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(updateModel.Description), "Description" },
                { new StringContent(updateModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(updateModel.Brand), "Brand" },
                { new StringContent(updateModel.Category), "Category" },
            };

            // Act
            var response = await client.PutAsync("/Promotions/1", formData);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedFlavours();
            db.SeedBrands();
            db.SeedCategories();
            db.SeedPackages();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
