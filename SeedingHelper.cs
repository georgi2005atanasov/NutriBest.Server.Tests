namespace NutriBest.Server.Tests
{
    using System.Net.Http.Json;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using NutriBest.Server.Features.Brands.Models;
    using NutriBest.Server.Features.Carts.Models;
    using NutriBest.Server.Features.GuestsOrders.Models;
    using NutriBest.Server.Features.Invoices.Models;
    using NutriBest.Server.Features.Products.Models;
    using NutriBest.Server.Features.PromoCodes.Models;
    using NutriBest.Server.Features.Promotions.Models;
    using NutriBest.Server.Features.ShippingDiscounts.Models;
    using NutriBest.Server.Features.UsersOrders.Models;

    public static class SeedingHelper
    {
        public static async Task SeedProduct(ClientHelper clientHelper,
            string productName,
            List<string> categories,
            string price,
            string brandName,
            string productSpecs)
        {
            var client = await clientHelper.GetAdministratorClientAsync();

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
            var client = await clientHelper.GetAdministratorClientAsync();

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

            var response = await client.PostAsync("/Promotions", formData);
            var data = await response.Content.ReadAsStringAsync();
        }

        public static async Task SeedShippingDiscount(ClientHelper clientHelper,
            string countryName,
            string description,
            string discountPercentage,
            DateTime? endDate,
            string? minPrice
            )
        {
            var client = await clientHelper.GetAdministratorClientAsync();

            var shippingModel = new CreateShippingDiscountServiceModel
            {
                CountryName = countryName,
                Description = description,
                DiscountPercentage = discountPercentage,
                EndDate = endDate,
                MinimumPrice = minPrice
            };

            var response = await client.PostAsJsonAsync("/ShippingDiscount", shippingModel);
            var data = await response.Content.ReadAsStringAsync();
        }

        public static async Task SeedPromoCode(ClientHelper clientHelper,
            string description,
            string count,
            string discountPercentage)
        {
            var promoCodeModel = new PromoCodeServiceModel
            {
                Count = count,
                DiscountPercentage = discountPercentage,
                Description = description
            };

            var client = await clientHelper.GetAdministratorClientAsync();

            await client.PostAsJsonAsync("/PromoCode", promoCodeModel);
        }

        public static async Task SeedBrand(ClientHelper clientHelper,
            string name,
            string description)
        {
            var client = await clientHelper.GetAdministratorClientAsync();

            var fakeImage = new Mock<IFormFile>();

            var brandModel = new CreateBrandServiceModel
            {
                Description = description,
                Name = name,
                Image = fakeImage.Object
            };

            // Convert the brand model to form-data content
            var formData = new MultipartFormDataContent
            {
                { new StringContent(brandModel.Description), "Description" },
                { new StringContent(brandModel.Name), "Name" }
            };

            using var ms = new MemoryStream();
            await brandModel.Image.CopyToAsync(ms);
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
            formData.Add(fileContent, "Image", "FakeName123");

            // Act
            await client.PostAsync("/Brands", formData);
        }

        public static async Task SeedSubscriber(ClientHelper clientHelper,
            string email)
        {
            var client = await clientHelper.GetAdministratorClientAsync();
            var formData = new MultipartFormDataContent
            {
                { new StringContent(email), "Email" }
            };

            var response = await client.PostAsync("/Newsletter", formData);
            var data = await response.Content.ReadAsStringAsync();
        }

        public static async Task SeedGuestOrder(ClientHelper clientHelper)
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var firstCartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 1,
                Price = 15.99m,
                ProductId = 1
            };

            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Set", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            var orderModel = new GuestOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "TEST_EMAIL@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832",
            };

            // Act
            var orderResponse = await client.PostAsJsonAsync("/GuestsOrders", orderModel);
        }

        public static async Task SeedUserOrder(ClientHelper clientHelper, bool hasInvoice)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var firstCartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = 1,
                Price = 15.99m,
                ProductId = 1
            };

            var secondCartProductModel = new CartProductServiceModel
            {
                Flavour = "Lemon Lime",
                Grams = 500,
                Count = 9,
                Price = 50.99m,
                ProductId = 3
            };

            // Act
            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Set", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Second product addition
            var secondResponse = await client.PostAsJsonAsync("/Cart/Set", secondCartProductModel);
            var updatedCookieHeaderAfterSecond = secondResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (updatedCookieHeaderAfterSecond != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", updatedCookieHeaderAfterSecond);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = hasInvoice,
                Invoice = hasInvoice ?
                          new InvoiceServiceModel
                          {
                              FirstName = "TEST",
                              LastName = "INVOICE",
                              Bullstat = "123456",
                              CompanyName = "TEST COMPANY",
                              PersonInCharge = "TEST PERSON IN CHARGE",
                              PhoneNumber = "0884138850"
                          } : 
                          null,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832"
            };

            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
        }

        public static async Task SeedUserOrderForSendingLowStockNotification(ClientHelper clientHelper,
            int quantity)
        {
            // Arrange
            var client = await clientHelper.GetOtherUserClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var firstCartProductModel = new CartProductServiceModel
            {
                Flavour = "Coconut",
                Grams = 1000,
                Count = quantity,
                Price = 15.99m,
                ProductId = 1
            };

            var secondCartProductModel = new CartProductServiceModel
            {
                Flavour = "Lemon Lime",
                Grams = 500,
                Count = 9,
                Price = 50.99m,
                ProductId = 3
            };

            // Act
            // First product addition
            var firstResponse = await client.PostAsJsonAsync("/Cart/Set", firstCartProductModel);
            var cookieHeader = firstResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (cookieHeader != null)
            {
                client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
            }

            // Second product addition
            var secondResponse = await client.PostAsJsonAsync("/Cart/Set", secondCartProductModel);
            var updatedCookieHeaderAfterSecond = secondResponse.Headers.GetValues("Set-Cookie").FirstOrDefault();
            if (updatedCookieHeaderAfterSecond != null)
            {
                client.DefaultRequestHeaders.Remove("Cookie");
                client.DefaultRequestHeaders.Add("Cookie", updatedCookieHeaderAfterSecond);
            }

            var orderModel = new UserOrderServiceModel
            {
                Country = "Bulgaria",
                City = "Plovdiv",
                Street = "Karlovska",
                StreetNumber = "900",
                PostalCode = "4000",
                Email = "user@example.com",
                Name = "TEST USER!!!",
                HasInvoice = false,
                PaymentMethod = "CashOnDelivery",
                PhoneNumber = "0884138832"
            };

            var orderResponse = await client.PostAsJsonAsync("/UsersOrders", orderModel);
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

        public static (MultipartFormDataContent formDataPercentDiscount,
                        MultipartFormDataContent formDataAmountDiscount)
                        GetTwoPromotions()
        {
            var promotionModelPercentDiscount = new CreatePromotionServiceModel
            {
                Brand = "NutriBest",
                Description = "TEST PROMO",
                Category = "Vitamins",
                StartDate = DateTime.Now,
                DiscountPercentage = "25",
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
