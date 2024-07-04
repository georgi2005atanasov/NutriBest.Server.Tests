namespace NutriBest.Server.Tests
{
    using System.Net.Http.Json;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NutriBest.Server.Features.Products.Models;
    using NutriBest.Server.Features.Promotions.Models;
    using NutriBest.Server.Features.ShippingDiscounts.Models;

    public static class SeedingHelper
    {
        public static async Task SeedProduct(ClientHelper clientHelper,
            string productName,
            List<string> categories,
            string price,
            string brandName,
            string productSpecs)
        {
            var productModel = new CreateProductRequestModel
            {
                Name = productName,
                Description = "this is product 1",
                Image = new Mock<IFormFile>().Object,
                Categories = categories,
                Brand = brandName,
                Price = price,
                ProductSpecs = productSpecs
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(productModel.Description), "Description" },
                { new StringContent(productModel.Name), "Name" },
                { new StringContent(productModel.Brand), "Brand" },
                { new StringContent(productModel.Price), "Price" },
                { new StringContent(productModel.ProductSpecs), "ProductSpecs" }
            };

            if (productModel.Image != null)
            {
                using var ms = new MemoryStream();
                await productModel.Image.CopyToAsync(ms);
                var fileContent = new ByteArrayContent(ms.ToArray());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                formData.Add(fileContent, "Image", "FakeName");
            }

            for (int i = 0; i < productModel.Categories.Count; i++)
            {
                formData.Add(new StringContent(productModel.Categories[i]), $"Categories[{i}]");
            }

            var client = await clientHelper.GetAdministratorClientAsync();
            var response = await client.PostAsync("/Products", formData);
            var data = await response.Content.ReadAsStringAsync();
        }

        public static async Task SeedPromotionWithDiscountPercentage(ClientHelper clientHelper, 
            string? brandName,
            string description,
            string? category,
            DateTime startDate,
            string discountPercentage)
        {
            var promotionModel = new CreatePromotionServiceModel
            {
                Brand = brandName,
                Description = description,
                Category = category,
                StartDate = startDate,
                DiscountPercentage = discountPercentage
            };

            var formData = new MultipartFormDataContent
            {
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.DiscountPercentage), "DiscountPercentage" },
                { new StringContent(promotionModel.Brand ?? ""), "Brand" },
                { new StringContent(promotionModel.StartDate.ToString("o")), "StartDate" },
                { new StringContent(promotionModel.Description), "Description" },
                { new StringContent(promotionModel.Category ?? ""), "Category" },
            };

            var client = await clientHelper.GetAdministratorClientAsync();
            var response = await client.PostAsync("/Promotions", formData);
            var data = await response.Content.ReadAsStringAsync();
        }

        public static async Task SeedShippingDiscount(ClientHelper clientHelper,
            string countryName,
            string description,
            string discountPercentage,
            DateTime? endDate,
            string minPrice
            )
        {
            var shippingModel = new CreateShippingDiscountServiceModel
            {
                CountryName = countryName,
                Description = description,
                DiscountPercentage = discountPercentage,
                EndDate = endDate,
                MinimumPrice = minPrice
            };

            var client = await clientHelper.GetAdministratorClientAsync();
            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingModel);
            var data = await response.Content.ReadAsStringAsync();
        }

        public static async Task SeedThreeProducts(ClientHelper clientHelper)
        {
            await SeedingHelper.SeedProduct(clientHelper,
                "product71",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product72",
                new List<string>
                {
                    "Vitamins"
                },
                "100",
                "Klean Athlete",
                "[{ \"flavour\": \"Cookies and Cream\", \"grams\": 250, \"quantity\": 100, \"price\": \"99.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product73",
                            new List<string>
                {
                    "Proteins"
                },
                "10",
                "Nordic Naturals",
                "[{ \"flavour\": \"Lemon Lime\", \"grams\": 500, \"quantity\": 100, \"price\": \"50.99\"}]");
        }

        public static async Task SeedSevenProducts(ClientHelper clientHelper)
        {
            await SeedingHelper.SeedProduct(clientHelper,
                "product80",
                            new List<string>
                {
                    "Creatines"
                },
                "15",
                "Klean Athlete",
                "[{ \"flavour\": \"Coconut\", \"grams\": 1000, \"quantity\": 100, \"price\": \"15.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product81",
                new List<string>
                {
                    "Vitamins"
                },
                "100",
                "Klean Athlete",
                "[{ \"flavour\": \"Cookies and Cream\", \"grams\": 250, \"quantity\": 100, \"price\": \"99.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product82",
                            new List<string>
                {
                    "Proteins"
                },
                "10",
                "Nordic Naturals",
                "[{ \"flavour\": \"Lemon Lime\", \"grams\": 500, \"quantity\": 100, \"price\": \"50.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product74",
                            new List<string>
                {
                    "Proteins"
                },
                "10",
                "Muscle Tech",
                "[{ \"flavour\": \"Banana\", \"grams\": 1500, \"quantity\": 100, \"price\": \"10.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product75",
                            new List<string>
                {
                    "Amino Acids"
                },
                "10",
                "Muscle Tech",
                "[{ \"flavour\": \"Chocolate\", \"grams\": 1000, \"quantity\": 100, \"price\": \"150.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product76",
                            new List<string>
                {
                    "Fish Oils"
                },
                "10",
                "Optimim Nutrition",
                "[{ \"flavour\": \"Chocolate\", \"grams\": 500, \"quantity\": 100, \"price\": \"500.99\"}]");

            await SeedingHelper.SeedProduct(clientHelper,
                "product77",
                            new List<string>
                {
                    "Vitamins"
                },
                "10",
                "NutriBest",
                "[{ \"flavour\": \"Cafe Latte\", \"grams\": 2000, \"quantity\": 100, \"price\": \"2000.99\"}]");
        }
    }
}
