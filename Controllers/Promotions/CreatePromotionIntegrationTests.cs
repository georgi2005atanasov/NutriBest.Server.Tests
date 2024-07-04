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

    [Collection("Promotions Controller Tests")]
    public class CreatePromotionIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CreatePromotionIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public async Task AllPromotionsEndpoint_ShouldBeExecuted_ForAdmin()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var (formDataPercentDiscount, formDataAmountDiscount) = GetTwoPromotions();

            // Act
            var firstResponse = await client.PostAsync("/Promotions", formDataPercentDiscount);
            var dataFirstResponse = await firstResponse.Content.ReadAsStringAsync();
            var secondResponse = await client.PostAsync("/Promotions", formDataAmountDiscount);
            var dataSecondResponse = await secondResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.Equal("1", dataFirstResponse);
            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
            Assert.Equal("2", dataSecondResponse);
        }

        [Fact]
        public async Task AllPromotionsEndpoint_ShouldBeExecuted_ForEmployee()
        {
            // Arrange
            var client = await clientHelper.GetEmployeeClientAsync();

            var (formDataPercentDiscount, formDataAmountDiscount) = GetTwoPromotions();

            // Act
            var firstResponse = await client.PostAsync("/Promotions", formDataPercentDiscount);
            var dataFirstResponse = await firstResponse.Content.ReadAsStringAsync();
            var secondResponse = await client.PostAsync("/Promotions", formDataAmountDiscount);
            var dataSecondResponse = await secondResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            Assert.Equal("1", dataFirstResponse);
            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
            Assert.Equal("2", dataSecondResponse);
        }

        [Fact]
        public async Task CreatePromotion_ShouldReturnBadRequest_WhenBothDiscountsAreChosen()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var promotionModel = new CreatePromotionServiceModel
            {
                Brand = "NutriBest",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now,
                DiscountPercentage = "25",
                DiscountAmount = "10"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.DiscountAmount), "DiscountAmount" },
                { new StringContent(promotionModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(promotionModel.Brand ?? ""), "Brand" },
                { new StringContent(promotionModel.StartDate.ToString("o")), "StartDate" },
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Category ?? ""), "Category" },
            };

            // Act
            var response = await client.PostAsync("/Promotions", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(db!.Promotions.Any(x => x.Description == "TEST PROMO"));
            Assert.Equal(TypeOfDiscountIsRequired, result.Message);
        }

        [Fact]
        public async Task CreatePromotion_ShouldReturnBadRequest_WhenDiscountIsNotChosen()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var promotionModel = new CreatePromotionServiceModel
            {
                Brand = "NutriBest",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now,
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Brand ?? ""), "Brand" },
                { new StringContent(promotionModel.StartDate.ToString("o")), "StartDate" },
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Category ?? ""), "Category" },
            };

            // Act
            var response = await client.PostAsync("/Promotions", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(db!.Promotions.Any(x => x.Description == "TEST PROMO"));
            Assert.Equal(DiscountIsRequired, result.Message);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("-100")]
        public async Task CreatePromotion_ShouldReturnBadRequest_WhenDiscountAmountIsBelowZero(
            string discountAmount)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var promotionModel = new CreatePromotionServiceModel
            {
                Brand = "NutriBest",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now,
                DiscountAmount = discountAmount
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Brand ?? ""), "Brand" },
                { new StringContent(promotionModel.StartDate.ToString("o")), "StartDate" },
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Category ?? ""), "Category" },
                { new StringContent(promotionModel.DiscountAmount), "DiscountAmount" },
            };

            // Act
            var response = await client.PostAsync("/Promotions", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(db!.Promotions.Any(x => x.Description == "TEST PROMO"));
            Assert.Equal(InvalidDiscount, result.Message);
        }

        [Theory]
        [InlineData("-1")]
        [InlineData("-100")]
        [InlineData("100")]
        [InlineData("101")]
        public async Task CreatePromotion_ShouldReturnBadRequest_WhenDiscountPercentageIsInvalid(
            string discountPercentage)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var promotionModel = new CreatePromotionServiceModel
            {
                Brand = "NutriBest",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now,
                DiscountPercentage = discountPercentage
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Brand ?? ""), "Brand" },
                { new StringContent(promotionModel.StartDate.ToString("o")), "StartDate" },
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Category ?? ""), "Category" },
                { new StringContent(promotionModel.DiscountPercentage), "DiscountPercentage" },
            };

            // Act
            var response = await client.PostAsync("/Promotions", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(db!.Promotions.Any(x => x.Description == "TEST PROMO"));
            Assert.Equal(InvalidDiscount, result.Message);
        }

        [Fact]
        public async Task CreatePromotion_ShouldReturnBadRequest_WhenEndDateIsBeforeToday()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var promotionModel = new CreatePromotionServiceModel
            {
                Brand = "NutriBest",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now.Subtract(TimeSpan.FromDays(2)),
                DiscountPercentage = "10"
            };

            var endDate = DateTime.Now.Subtract(TimeSpan.FromDays(1));

            var formData = new MultipartFormDataContent
            {
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Brand ?? ""), "Brand" },
                { new StringContent(promotionModel.StartDate.ToString("o")), "StartDate" },
                { new StringContent(endDate.ToString("o")), "EndDate" },
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Category ?? ""), "Category" },
                { new StringContent(promotionModel.DiscountPercentage), "DiscountPercentage" },
            };

            // Act
            var response = await client.PostAsync("/Promotions", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(db!.Promotions.Any(x => x.Description == "TEST PROMO"));
            Assert.Equal(LeastPromotionDurationRequired, result.Message);
        }

        [Fact]
        public async Task CreatePromotion_ShouldReturnBadRequest_WhenStartDateIsBeforeEndDate()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var promotionModel = new CreatePromotionServiceModel
            {
                Brand = "NutriBest",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now,
                DiscountPercentage = "10"
            };

            var endDate = DateTime.Now.Subtract(TimeSpan.FromDays(1));

            var formData = new MultipartFormDataContent
            {
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Brand ?? ""), "Brand" },
                { new StringContent(promotionModel.StartDate.ToString("o")), "StartDate" },
                { new StringContent(endDate.ToString("o")), "EndDate" },
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Category ?? ""), "Category" },
                { new StringContent(promotionModel.DiscountPercentage), "DiscountPercentage" },
            };

            // Act
            var response = await client.PostAsync("/Promotions", formData);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var result = JsonSerializer.Deserialize<FailResponse>(data, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new FailResponse();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(db!.Promotions.Any(x => x.Description == "TEST PROMO"));
            Assert.Equal(StartDateMustBeBeforeEndDate, result.Message);
        }

        [Fact]
        public async Task CreatePromotion_ShouldReturnBadRequest_WhenPromotionAlreadyExists()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var promotionModel = new CreatePromotionServiceModel
            {
                Brand = "NutriBest",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now,
                DiscountPercentage = "10"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Brand ?? ""), "Brand" },
                { new StringContent(promotionModel.StartDate.ToString("o")), "StartDate" },
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Category ?? ""), "Category" },
                { new StringContent(promotionModel.DiscountPercentage), "DiscountPercentage" },
            };

            // Act
            await client.PostAsync("/Promotions", formData);
            var response = await client.PostAsync("/Promotions", formData);
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
        public async Task CreatePromotion_ShouldReturnBadRequest_WhenInvalidBrandIsPassed()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var promotionModel = new CreatePromotionServiceModel
            {
                Brand = "InvalidBrand",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now,
                DiscountPercentage = "10"
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Brand ?? ""), "Brand" },
                { new StringContent(promotionModel.StartDate.ToString("o")), "StartDate" },
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Category ?? ""), "Category" },
                { new StringContent(promotionModel.DiscountPercentage), "DiscountPercentage" },
            };

            // Act
            var response = await client.PostAsync("/Promotions", formData);
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
        public async Task AllPromotionsEndpoint_ShouldReturnUnauthorized_ForAnonymous()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            var (formDataPercentDiscount, formDataAmountDiscount) = GetTwoPromotions();

            // Act
            var firstResponse = await client.PostAsync("/Promotions", formDataPercentDiscount);
            var secondResponse = await client.PostAsync("/Promotions", formDataAmountDiscount);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, secondResponse.StatusCode);
        }

        [Fact]
        public async Task AllPromotionsEndpoint_ShouldReturnForbidden_ForUsers()
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            var (formDataPercentDiscount, formDataAmountDiscount) = GetTwoPromotions();

            // Act
            var firstResponse = await client.PostAsync("/Promotions", formDataPercentDiscount);
            var secondResponse = await client.PostAsync("/Promotions", formDataAmountDiscount);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, firstResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, secondResponse.StatusCode);
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

        private static (MultipartFormDataContent formDataPercentDiscount, 
                        MultipartFormDataContent formDataAmountDiscount) 
                        GetTwoPromotions()
        {
            var promotionModelPercentDiscount = new CreatePromotionServiceModel
            {
                Brand = "NutriBest",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now,
                DiscountPercentage = "25"
            };

            var promotionModelAmountDiscount = new CreatePromotionServiceModel
            {
                Brand = "Klean Athlete",
                Description = "TEST PROMO2",
                Category = null,
                StartDate = DateTime.Now,
                DiscountAmount = "10",
            };

            var formDataPercentDiscount = new MultipartFormDataContent
            {
                { new StringContent(promotionModelPercentDiscount.Description), "Description" },
                { new StringContent(promotionModelPercentDiscount.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(promotionModelPercentDiscount.Brand), "Brand" },
                { new StringContent(promotionModelPercentDiscount.StartDate.ToString("o")), "StartDate" },
                { new StringContent(promotionModelPercentDiscount.Description), "Description" },
                { new StringContent(promotionModelPercentDiscount.Category), "Category" },
            };

            var formDataAmountDiscount = new MultipartFormDataContent
            {
                { new StringContent(promotionModelAmountDiscount.Description), "Description" },
                { new StringContent(promotionModelAmountDiscount.DiscountAmount), "DiscountAmount" },
                { new StringContent(promotionModelAmountDiscount.Brand), "Brand" },
                { new StringContent(promotionModelAmountDiscount.StartDate.ToString("o")), "StartDate" },
                { new StringContent(promotionModelAmountDiscount.Description), "Description" },
                { new StringContent(promotionModelAmountDiscount.Category ?? ""), "Category" },
            };

            return (formDataPercentDiscount, formDataAmountDiscount);
        }
    }
}
