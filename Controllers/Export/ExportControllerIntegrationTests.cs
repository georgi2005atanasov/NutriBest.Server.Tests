namespace NutriBest.Server.Tests.Controllers.Export
{
    using System.Net;
    using System.Text.Json;
    using System.Reflection;
    using System.Net.Http.Json;
    using System.Text.RegularExpressions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using Moq;
    using NutriBest.Server.Data;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Features.Export;
    using NutriBest.Server.Features.Email.Models;
    using NutriBest.Server.Infrastructure.Extensions;
    using NutriBest.Server.Tests.Controllers.Export.Data;
    using NutriBest.Server.Features.ShippingDiscounts.Models;
    using NutriBest.Server.Features.UsersOrders.Models;
    using NutriBest.Server.Features.Carts.Models;

    [Collection("Export Controller and Service Tests")]
    public class ExportControllerIntegrationTests : IAsyncLifetime
    {
        private NutriBestDbContext? db;

        private ClientHelper clientHelper;

        private CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public ExportControllerIntegrationTests(CustomWebApplicationFactoryFixture fixture)
        {
            clientHelper = new ClientHelper(fixture);
            this.fixture = fixture;
        }

        [Fact]
        public void ExportController_ShouldHave_AuthorizeAttributeWithRoles()
        {
            var type = typeof(ExportController);

            var authorizeAttribute = type.GetCustomAttribute<AuthorizeAttribute>();

            Assert.NotNull(authorizeAttribute);

            var roles = authorizeAttribute!.Roles!.Split(',');

            Assert.Contains("Administrator", roles);
            Assert.Contains("Employee", roles);
        }

        [Fact]
        public async Task GetCsvBrands_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Brands/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert

            Assert.Equal("BrandName,Description\r\n\"Garden of Life\",-\r\n\"Klean Athlete\",-\r\n\"Muscle Tech\",-\r\n\"Nature Made\",-\r\n\"Nordic Naturals\",-\r\nNutriBest,-\r\n\"Optimim Nutrition\",-\r\n",
                         data);
        }

        [Fact]
        public async Task GetCsvCategories_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Categories/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var expectedCsvText = "CategoryName\r\n\"Amino Acids\"\r\nCreatines\r\n\"Fat Burners\"\r\n\"Fish Oils\"\r\n\"Mass Gainers\"\r\nPost-Workout\r\nPre-Workout\r\nPromotions\r\nProteins\r\nRecovery\r\nVegan\r\nVitamins\r\n";

            Assert.Equal(expectedCsvText, data);
        }

        [Fact]
        public async Task GetCsvFlavours_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Flavours/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var expectedCsvText = "FlavourName\r\nBanana\r\nBlueberry\r\n\"Cafe Latte\"\r\nChocolate\r\n\"Cinnamon Roll\"\r\nCoconut\r\n\"Cookies and Cream\"\r\n\"Lemon Lime\"\r\nMango\r\nMatcha\r\n\"Mint Chocolate\"\r\n\"Peanut Butter\"\r\n\"Salted Caramel\"\r\nStrawberry\r\nVanilla\r\n";

            Assert.Equal(expectedCsvText, data);
        }

        [Fact]
        public async Task GetCsvNewsletter_ShouldBeExecuted_WithoutFilters()
        {
            // Arrange
            var client = clientHelper.GetAnonymousClient();

            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                {
                    var formData = new MultipartFormDataContent
                    {
                        { new StringContent($"pesho{i}@example.com"), "email" },
                    };

                    await client.PostAsync("/Newsletter", formData);
                }
            }

            var admin = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await admin.GetAsync("/Newsletter/CSV");
            var data = await response.Content.ReadAsStringAsync();

            var regex = new Regex(@"""\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}""");
            var modifiedCsvText = regex.Replace(data, "\"PLACEHOLDER_DATE\"");

            // Assert
            var expectedCsvText = "Email,Name,RegisteredOn,PhoneNumber,IsAnonymous,HasOrders,TotalOrders\r\n" +
                                  "pesho8@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n" +
                                  "pesho6@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n" +
                                  "pesho4@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n" +
                                  "pesho2@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n" +
                                  "pesho0@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n";

            Assert.Equal(expectedCsvText, modifiedCsvText);
        }

        [Theory]
        [MemberData(nameof(ExportTestFilters.NewsletterFilters), MemberType = typeof(ExportTestFilters))]
        public async Task GetCsvNewsletter_ShouldBeExecuted_WithFilters(
            string? search,
            string? groupType,
            string expectedOutput)
        {
            // Arrange
            var admin = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUsersAsSubscribers(clientHelper, false, 10);

            // seed also 'user' in order to have somebody with orders
            var anonymous = clientHelper.GetAnonymousClient();

            var formData = new MultipartFormDataContent
            {
                { new StringContent("user@example.com"), "email" },
            };

            await anonymous.PostAsync("/Newsletter", formData);

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "user@example.com",
                "user",
                "TEST USER!!!");

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "UNIQUE_USER_3@example.com",
                "3_UNIQUE_USER",
                "Some name");

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "UNIQUE_USER_4@example.com",
                "4_UNIQUE_USER",
                "Some name");

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "UNIQUE_USER_5@example.com",
                "5_UNIQUE_USER",
                "Some name");

            var query = $"?search={search}&groupType={groupType}";

            // Act
            var response = await admin.GetAsync($"/Newsletter/CSV{query}");
            var data = await response.Content.ReadAsStringAsync();

            var regex = new Regex(@"""\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}""");
            var modifiedCsvText = regex.Replace(data, "\"PLACEHOLDER_DATE\"");

            var expectedModifiedText = regex.Replace(expectedOutput, "\"PLACEHOLDER_DATE\"");

            Assert.Equal(expectedModifiedText, modifiedCsvText);
        }


        [Fact]
        public async Task GetCsvPackages_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Packages/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var expectedCsvText = "Grams\r\n100\r\n250\r\n500\r\n750\r\n1000\r\n1500\r\n2000\r\n2500\r\n";

            Assert.Equal(expectedCsvText, data);
        }

        [Fact]
        public async Task GetCsvProducts_ShouldBeExecuted_WithoutFilters()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedThreeProducts(clientHelper);

            // Act
            var response = await client.GetAsync("/Products/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var expectedCsvText = "Id,Name,StartingPrice,Brand,Description,Categories,PromotionId\r\n3,product73,\"50,99 BGN\",\"Nordic Naturals\",\"this is product 1\",Proteins,-\r\n2,product72,\"99,99 BGN\",\"Klean Athlete\",\"this is product 1\",Vitamins,-\r\n1,product71,\"15,99 BGN\",\"Klean Athlete\",\"this is product 1\",Creatines,-\r\n";

            Assert.Equal(expectedCsvText, data);
        }

        [Theory]
        [MemberData(nameof(ExportTestFilters.ProductsFilters), MemberType = typeof(ExportTestFilters))]
        public async Task GetCsvProducts_ShouldBeExecuted_WithFilters(
            string? priceOrder,
            string? alphaOrder,
            string? brand,
            string? categories,
            string? search,
            string? quantities,
            string? priceRange,
            string expectedOutput)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await SeedingHelper.SeedSevenProducts(clientHelper);

            var url = "/Products/CSV?page=1";

            if (!string.IsNullOrEmpty(priceOrder))
                url += $"&price={priceOrder}";
            if (!string.IsNullOrEmpty(alphaOrder))
                url += $"&alpha={alphaOrder}";
            if (!string.IsNullOrEmpty(brand))
                url += $"&brand={brand}";
            if (!string.IsNullOrEmpty(categories))
                url += $"&categories={categories}";
            if (!string.IsNullOrEmpty(search))
                url += $"&search={search}";
            if (!string.IsNullOrEmpty(quantities))
                url += $"&quantities={quantities}";
            if (!string.IsNullOrEmpty(priceRange))
                url += $"&priceRange={priceRange}";

            // Act
            var response = await client.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(expectedOutput, data);
        }

        [Fact]
        public async Task GetCsvProfiles_ShouldBeExecuted_WithoutFilters()
        {
            // Arrange
            await LargeSeedingHelper.SeedUsers(clientHelper, 10);

            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/Profiles/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var expectedCsvText = "Id,Name,Email,MadeOn,IsDeleted,PhoneNumber,City,TotalOrders,TotalSpent\r\n5297600a-7602-43f8-8054-0766b5db23c1,-,UNIQUE_USER_9@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\n8c297f41-59dd-439e-b268-73af415fa115,-,UNIQUE_USER_8@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\n0bf366e3-d2cb-483f-be88-e26b5286a33d,-,UNIQUE_USER_7@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\n6d90ea45-838f-4a32-a47d-7b84d0e7e311,-,UNIQUE_USER_6@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\n6f366e96-e5b9-43e2-ae5d-0408e8014203,-,UNIQUE_USER_5@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\ncfd9a7f5-d981-4313-bfb4-4fb699b95196,-,UNIQUE_USER_4@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\n2a1fdd30-b9b1-46b2-9cce-a8479f431673,-,UNIQUE_USER_3@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\n15e56fb2-c698-4dce-a7f0-040a3648d380,-,UNIQUE_USER_2@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\na769b597-0bdb-453e-903e-68e84be135b3,-,UNIQUE_USER_1@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\nfca6edc3-cbd3-48c4-8fcc-0e4d7aa39710,-,UNIQUE_USER_0@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\n643bd70e-f0cd-4a32-8ecb-429ae848b8fb,-,user@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\nbc74d62b-5581-4752-9edd-ea182e666209,-,employee@example.com,\"10.7.2024 г. 20:13:32\",False,-,-,0,0\r\n0bc14132-2c81-4425-9e4a-1efeb408d287,-,admin@example.com,\"10.7.2024 г. 20:13:32\",False,+359884138832,-,0,0\r\n";

            var dateRegex = new Regex(@"\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}");
            var idRegex = new Regex(@"[0-9a-fA-F-]{36}");

            var modifiedCsvText = dateRegex.Replace(data, "PLACEHOLDER_DATE");
            modifiedCsvText = idRegex.Replace(modifiedCsvText, "PLACEHOLDER_ID");

            var modifiedExpectedText = dateRegex.Replace(expectedCsvText, "PLACEHOLDER_DATE");
            modifiedExpectedText = idRegex.Replace(modifiedExpectedText, "PLACEHOLDER_ID");

            Assert.Equal(modifiedExpectedText, modifiedCsvText);
        }

        [Theory]
        [MemberData(nameof(ExportTestFilters.ProfileFilters), MemberType = typeof(ExportTestFilters))]
        public async Task GetCsvProfiles_ShouldBeExecuted_WithFilters(
            string? search,
            string? groupType,
            string expectedOutput)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUsers(clientHelper, 20);

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "user@example.com",
                "user",
                "TEST USER!!!");

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "UNIQUE_USER_3@example.com",
                "3_UNIQUE_USER",
                "Some name");

            await SeedingHelper.SeedUserOrder(clientHelper,
                false,
                "UNIQUE_USER_4@example.com",
                "4_UNIQUE_USER",
                "Some name");

            // Act
            var response = await client.GetAsync($"/Profiles/CSV?search={search}&groupType={groupType}");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var dateRegex = new Regex(@"\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}");
            var idRegex = new Regex(@"[0-9a-fA-F-]{36}");

            var modifiedCsvText = dateRegex.Replace(data, "PLACEHOLDER_DATE");
            modifiedCsvText = idRegex.Replace(modifiedCsvText, "PLACEHOLDER_ID");

            var modifiedExpectedText = dateRegex.Replace(expectedOutput, "PLACEHOLDER_DATE");
            modifiedExpectedText = idRegex.Replace(modifiedExpectedText, "PLACEHOLDER_ID");

            Assert.Equal(modifiedExpectedText, modifiedCsvText);
        }

        [Fact]
        public async Task GetCsvPromotions_ShouldBeExecuted()
        {
            // Arrange
            await SeedingHelper.SeedPromotionWithDiscountPercentage(clientHelper,
                null,
                "20% OFF!",
                null,
                DateTime.Now,
                "20");

            var (_, formDataAmountDiscount) = SeedingHelper.GetTwoPromotions();

            var client = await clientHelper.GetAdministratorClientAsync();

            await client.PostAsync("/Promotions", formDataAmountDiscount);

            // Act
            var response = await client.GetAsync("/Promotions/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var expectedCsvText = "Id,Description,Brand,Category,DiscountAmount,DiscountPercentage,IsActive,StartDate,EndDate\r\n2,\"TEST PROMO2\",\"Klean Athlete\",-,\"10 BGN\",-,False,\"10.7.2024 г. 21:00:49\",\r\n1,\"20% OFF!\",-,-,-,20%,False,\"10.7.2024 г. 21:00:48\",\r\n";

            var regex = new Regex(@"""\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}""");
            var modifiedCsvText = regex.Replace(data, "\"PLACEHOLDER_DATE\"");
            var expectedModifiedText = regex.Replace(expectedCsvText, "\"PLACEHOLDER_DATE\"");

            Assert.Equal(expectedModifiedText, modifiedCsvText);
        }

        [Fact]
        public async Task GetCsvPromoCodes_ShouldBeExecuted()
        {
            // Arrange
            db!.PromoCodes.Add(new PromoCode
            {
                Code = "ASDFGHG",
                Description = "20% OFF",
                DiscountPercentage = 20,
                IsValid = true,
                IsSent = false,
            });

            await db.SaveChangesAsync();

            var client = await clientHelper.GetAdministratorClientAsync();

            // Act
            var response = await client.GetAsync("/PromoCodes/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var expectedCsvText = "Description,ExpiresIn,Codes\r\n\"20% OFF\",10,ASDFGHG\r\n";

            var regex = new Regex(@"""\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}""");
            var modifiedCsvText = regex.Replace(data, "\"PLACEHOLDER_DATE\"");
            var expectedModifiedText = regex.Replace(expectedCsvText, "\"PLACEHOLDER_DATE\"");

            Assert.Equal(expectedModifiedText, modifiedCsvText);
        }

        [Fact]
        public async Task GetCsvShippingDiscounts_ShouldBeExecuted()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            var shippingDiscountModel = new CreateShippingDiscountServiceModel
            {
                CountryName = "Bulgaria",
                Description = "TEST DISCOUNT",
                DiscountPercentage = "100",
                EndDate = null,
                MinimumPrice = "100"
            };

            await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);
            shippingDiscountModel.CountryName = "Germany";
            shippingDiscountModel.Description = "TEST DISCOUNT 2";
            shippingDiscountModel.EndDate = DateTime.Now;
            await client.PostAsJsonAsync("/ShippingDiscount", shippingDiscountModel);

            // Act
            var response = await client.GetAsync("/ShippingDiscounts/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var expectedCsvText = "DiscountPercentage,Country,Description,MinimumPrice,EndDate\r\n100,Bulgaria,\"TEST DISCOUNT\",\"100 BGN\",\r\n100,Germany,\"TEST DISCOUNT 2\",\"100 BGN\",\"11.7.2024 г. 0:22:40\"\r\n";

            var regex = new Regex(@"""\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}""");
            var modifiedCsvText = regex.Replace(data, "\"PLACEHOLDER_DATE\"");
            var expectedModifiedText = regex.Replace(expectedCsvText, "\"PLACEHOLDER_DATE\"");

            Assert.Equal(expectedModifiedText, modifiedCsvText);
        }

        [Fact]
        public async Task GetCsvOrders_ShouldBeExecuted_WithoutFilters()
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUserOrders(db!,
                clientHelper,
                30,
                new List<CartProductServiceModel>
                {
                    new CartProductServiceModel
                    {
                        Flavour = "Coconut",
                        Grams = 1000,
                        Count = 1,
                        Price = 15.99m,
                        ProductId = 1
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Lemon Lime",
                        Grams = 500,
                        Count = 2,
                        Price = 50.99m,
                        ProductId = 3
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Chocolate",
                        Grams = 500,
                        Count = 3,
                        Price = 500.99m,
                        ProductId = 6
                    }
                },
                new UserOrderServiceModel
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
                });

            // Act
            var response = await client.GetAsync("/Orders/CSV");
            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var expectedCsvText = "Id,IsFinished,IsConfirmed,MadeOn,CustomerName,City,Country,Email,PhoneNumber,IsPaid,IsShipped,PaymentMethod,IsAnonymous,TotalPrice\r\n30,False,False,\"10.7.2024 г. 21:31:20\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1630,94\"\r\n29,False,False,\"10.7.2024 г. 21:31:20\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1630,94\"\r\n28,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1306,752\"\r\n27,False,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,True,CashOnDelivery,False,\"1630,94\"\r\n26,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1622,945\"\r\n25,True,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,True,True,CashOnDelivery,False,\"1300,3560\"\r\n24,False,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,True,CashOnDelivery,False,\"1622,945\"\r\n23,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1622,945\"\r\n22,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1300,3560\"\r\n21,False,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,True,CashOnDelivery,False,\"1630,94\"\r\n20,True,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,True,True,CashOnDelivery,False,\"1630,94\"\r\n19,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1306,752\"\r\n18,False,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,True,CashOnDelivery,False,\"1630,94\"\r\n17,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1630,94\"\r\n16,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1306,752\"\r\n15,False,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,True,CashOnDelivery,False,\"1630,94\"\r\n14,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1630,94\"\r\n13,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1306,752\"\r\n12,False,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,True,CashOnDelivery,False,\"1630,94\"\r\n11,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1622,945\"\r\n10,True,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,True,True,CashOnDelivery,False,\"1300,3560\"\r\n9,False,True,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,True,CashOnDelivery,False,\"1622,945\"\r\n8,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1622,945\"\r\n7,False,False,\"10.7.2024 г. 21:31:19\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1300,3560\"\r\n6,False,True,\"10.7.2024 г. 21:31:18\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,True,CashOnDelivery,False,\"1630,94\"\r\n5,True,True,\"10.7.2024 г. 21:31:18\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,True,True,CashOnDelivery,False,\"1630,94\"\r\n4,False,False,\"10.7.2024 г. 21:31:18\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1306,752\"\r\n3,False,True,\"10.7.2024 г. 21:31:18\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,True,CashOnDelivery,False,\"1630,94\"\r\n2,False,False,\"10.7.2024 г. 21:31:18\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1630,94\"\r\n1,False,False,\"10.7.2024 г. 21:31:18\",\"TEST USER!!!29\",Plovdiv,Bulgaria,user@example.com,0884138832,False,False,CashOnDelivery,False,\"1306,752\"\r\n";
            var regex = new Regex(@"""\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}""");
            var modifiedCsvText = regex.Replace(data, "\"PLACEHOLDER_DATE\"");
            var expectedModifiedText = regex.Replace(expectedCsvText, "\"PLACEHOLDER_DATE\"");

            Assert.Equal(expectedModifiedText, modifiedCsvText);
        }

        [Theory]
        [MemberData(nameof(ExportTestFilters.OrdersFilters), MemberType = typeof(ExportTestFilters))]
        public async Task GetCsvOrders_ShouldBeExecuted_WithFilters(string? search,
            string? filters,
            string? startDate,
            string? endDate,
            string expectedCsvText)
        {
            // Arrange
            var client = await clientHelper.GetAdministratorClientAsync();

            await LargeSeedingHelper.SeedUserOrders(db!,
                clientHelper,
                30,
                new List<CartProductServiceModel>
                {
                    new CartProductServiceModel
                    {
                        Flavour = "Coconut",
                        Grams = 1000,
                        Count = 1,
                        Price = 15.99m,
                        ProductId = 1
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Lemon Lime",
                        Grams = 500,
                        Count = 2,
                        Price = 50.99m,
                        ProductId = 3
                    },
                    new CartProductServiceModel
                    {
                        Flavour = "Chocolate",
                        Grams = 500,
                        Count = 3,
                        Price = 500.99m,
                        ProductId = 6
                    }
                },
                new UserOrderServiceModel
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
                });

            // Act
            var response = await client
                .GetAsync($"/Orders/CSV?&filters={filters}&startDate={startDate ?? ""}&endDate={endDate ?? ""}&search={search}");

            var data = await response.Content.ReadAsStringAsync();

            // Assert
            var regex = new Regex(@"""\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}""");
            var modifiedCsvText = regex.Replace(data, "\"PLACEHOLDER_DATE\"");
            var expectedModifiedText = regex.Replace(expectedCsvText, "\"PLACEHOLDER_DATE\"");

            Assert.Equal(expectedModifiedText, modifiedCsvText);
        }

        public async Task InitializeAsync()
        {
            await fixture.ResetDatabaseAsync();
            fixture.Factory.EmailServiceMock!.Reset();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
            db.SeedDatabase(scope);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
