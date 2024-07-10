namespace NutriBest.Server.Tests.Controllers.Export
{
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using Xunit;
    using NutriBest.Server.Tests.Fixtures;
    using NutriBest.Server.Infrastructure.Extensions;
    using NutriBest.Server.Features.GuestsOrders.Models;
    using NutriBest.Server.Data.Models;

    public class ExportServiceTests : IClassFixture<ExportFixture>, IAsyncLifetime
    {
        private ExportFixture fixture;

        public ExportServiceTests(ExportFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        public async Task ExportBrand_ShouldBeExecuted()
        {
            // Arrange
            fixture.DbContext!.SeedBrands();
            var brands = await fixture.BrandService!.All();

            // Act
            var csvText = fixture.ExportService!.BrandsCsv(brands
                     .OrderBy(x => x.Name));

            // Assert
            string expectedCsvText = "BrandName,Description\r\n\"Garden of Life\",-\r\n\"Klean Athlete\",-\r\n\"Muscle Tech\",-\r\n\"Nature Made\",-\r\n\"Nordic Naturals\",-\r\nNutriBest,-\r\n\"Optimim Nutrition\",-\r\n";

            Assert.Equal(expectedCsvText, csvText);
        }

        [Fact]
        public async Task ExportCategories_ShouldBeExecuted()
        {
            // Arrange
            fixture.DbContext!.SeedCategories();
            var categories = await fixture.CategoryService!.All();

            // Act
            var csvText = fixture.ExportService!.CategoriesCsv(categories
                    .OrderBy(x => x.Name));

            // Assert
            var expectedCsvText = "CategoryName\r\n\"Amino Acids\"\r\nCreatines\r\n\"Fat Burners\"\r\n\"Fish Oils\"\r\n\"Mass Gainers\"\r\nPost-Workout\r\nPre-Workout\r\nPromotions\r\nProteins\r\nRecovery\r\nVegan\r\nVitamins\r\n";

            Assert.Equal(expectedCsvText, csvText);
        }

        [Fact]
        public async Task ExportFlavours_ShouldBeExecuted()
        {
            // Arrange
            fixture.DbContext!.SeedFlavours();
            var flavours = await fixture.FlavourService!.All();

            // Act
            var csvText = fixture.ExportService!
                .FlavoursCsv(flavours
                .OrderBy(x => x.Name));

            // Assert
            var expectedCsvText = "FlavourName\r\nBanana\r\nBlueberry\r\n\"Cafe Latte\"\r\nChocolate\r\n\"Cinnamon Roll\"\r\nCoconut\r\n\"Cookies and Cream\"\r\n\"Lemon Lime\"\r\nMango\r\nMatcha\r\n\"Mint Chocolate\"\r\n\"Peanut Butter\"\r\n\"Salted Caramel\"\r\nStrawberry\r\nVanilla\r\n";

            Assert.Equal(expectedCsvText, csvText);
        }

        [Fact]
        public async Task ExportNewsletter_ShouldBeExecuted()
        {
            // Arrange
            for (int i = 0; i < 10; i++)
            {
                if (i % 2 == 0)
                {
                    await fixture.NewsletterService!
                        .Add($"pesho{i}@example.com");
                }
            }

            var result = await fixture
                .NewsletterService!
                .AllSubscribers(1, null, null);

            // Act
            var csvText = fixture.ExportService!.NewsletterCsv(result.Subscribers
                .OrderBy(x => x.Name));

            // Replace the dates with a placeholder including quotation marks
            var regex = new Regex(@"""\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}""");
            var modifiedCsvText = regex.Replace(csvText, "\"PLACEHOLDER_DATE\"");

            // Assert
            var expectedCsvText = "Email,Name,RegisteredOn,PhoneNumber,IsAnonymous,HasOrders,TotalOrders\r\n" +
                                  "pesho8@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n" +
                                  "pesho6@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n" +
                                  "pesho4@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n" +
                                  "pesho2@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n" +
                                  "pesho0@example.com,,\"PLACEHOLDER_DATE\",,True,False,0\r\n";

            Assert.Equal(expectedCsvText, modifiedCsvText);
        }

        // ORDERS IN THE CONTROLLER ONLY!

        [Fact]
        public async Task ExportPackages_ShouldBeExecuted()
        {
            // Arrange
            fixture.DbContext!.SeedPackages();
            var packages = await fixture.PackageService!.All();

            // Act
            var csvText = fixture.ExportService!.PackagesCsv(packages
                .OrderBy(x => x.Grams));

            // Assert
            var expectedCsvText = "Grams\r\n100\r\n250\r\n500\r\n750\r\n1000\r\n1500\r\n2000\r\n2500\r\n";

            Assert.Equal(expectedCsvText, csvText);
        }

        [Fact]
        public async Task ExportProducts_ShouldBeExecuted()
        {
            // Arrange
            fixture.DbContext!.SeedBrands();
            fixture.DbContext!.SeedCategories();
            fixture.DbContext!.SeedFlavours();
            fixture.DbContext!.SeedPackages();

            for (int i = 0; i < 5; i++)
            {
                await fixture.ProductService!.Create($"product{i}",
                    "Some description",
                    "Klean Athlete",
                    new List<int>
                    {
                        1
                    },
                    new List<Features.Products.Models.ProductSpecsServiceModel>
                    {
                        new Features.Products.Models.ProductSpecsServiceModel
                        {
                            Flavour = "Chocolate",
                            Grams = 1000,
                            Price = "90",
                            Quantity = 10
                        }
                    },
                    "someData",
                    "image/png");
            }

            var products = await fixture.ProductService!.All(1,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

            // Act
            var csvText = fixture.ExportService!
                          .ProductsCsv(products.ProductsRows!
                          .SelectMany(x => x)
                          .OrderByDescending(x => x.ProductId));

            // Assert
            var expectedCsvText = "Id,Name,StartingPrice,Brand,Description,Categories,PromotionId\r\n5,product4,\"90 BGN\",\"Klean Athlete\",\"Some description\",Proteins,-\r\n4,product3,\"90 BGN\",\"Klean Athlete\",\"Some description\",Proteins,-\r\n3,product2,\"90 BGN\",\"Klean Athlete\",\"Some description\",Proteins,-\r\n2,product1,\"90 BGN\",\"Klean Athlete\",\"Some description\",Proteins,-\r\n1,product0,\"90 BGN\",\"Klean Athlete\",\"Some description\",Proteins,-\r\n";

            Assert.Equal(expectedCsvText, csvText);
        }

        [Fact]
        public async Task ProfileExport_ShouldBeExecuted()
        {
            // Arrange
            for (int i = 0; i < 3; i++)
            {
                fixture.DbContext!.Users.Add(new User
                {
                    UserName = $"pesho{i}",
                    Email = $"pesho{i}@example.com",
                    PasswordHash = "pesho"
                });

            }
            await fixture.DbContext!.SaveChangesAsync();

            var profiles = await fixture.ProfileService!
                .All(1,
                null,
                null);

            // Act
            var csvText = fixture.ExportService!
                          .ProfilesCsv(profiles.Profiles
                          .OrderByDescending(x => x.MadeOn));

            var dateRegex = new Regex(@"\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}");
            var idRegex = new Regex(@"[0-9a-fA-F-]{36}");

            var modifiedCsvText = dateRegex.Replace(csvText, "PLACEHOLDER_DATE");
            modifiedCsvText = idRegex.Replace(modifiedCsvText, "PLACEHOLDER_ID");

            // Assert
            var expectedCsvText = "Id,Name,Email,MadeOn,IsDeleted,PhoneNumber,City,TotalOrders,TotalSpent\r\n" +
                                  "PLACEHOLDER_ID,-,pesho0@example.com,\"PLACEHOLDER_DATE\",False,-,-,0,0\r\n" +
                                  "PLACEHOLDER_ID,-,pesho1@example.com,\"PLACEHOLDER_DATE\",False,-,-,0,0\r\n" +
                                  "PLACEHOLDER_ID,-,pesho2@example.com,\"PLACEHOLDER_DATE\",False,-,-,0,0\r\n";

            Assert.Equal(expectedCsvText, modifiedCsvText);
        }

        [Fact]
        public async Task PromoCodeExport_ShouldBeExecuted()
        {
            // Arrange
            fixture.DbContext!.PromoCodes.Add(new PromoCode
            {
                Code = "ASDFGHG",
                Description = "20% OFF",
                DiscountPercentage = 20,
                IsValid = true,
                IsSent = false,
            });

            await fixture.DbContext.SaveChangesAsync();

            var promoCodes = await fixture.PromoCodeService!
                                 .All();

            // Act
            var csvText = fixture.ExportService!
                          .PromoCodesCsv(promoCodes
                          .OrderBy(x => x.Description));

            // Assert
            var expectedCsvText = "Description,ExpiresIn,Codes\r\n\"20% OFF\",10,ASDFGHG\r\n";

            Assert.Equal(expectedCsvText, csvText);
        }

        [Fact]
        public async Task PromotionExport_ShouldBeExecuted()
        {
            // Arrange
            for (int i = 0; i < 3; i++)
            {
                fixture.DbContext!.Promotions!.Add(new Promotion
                {
                    Description = "20% OFF",
                    Brand = null,
                    Category = null,
                    DiscountAmount = i == 1 ? 5 : null,
                    DiscountPercentage = i == 0 ? 20 : null,
                    IsActive = false,
                    StartDate = DateTime.Now,
                    EndDate = i == 2 ? (DateTime?)null : DateTime.Now
                });
            }

            await fixture.DbContext!.SaveChangesAsync();

            var promotions = await fixture.PromotionService!
                                 .All();

            // Act
            var csvText = fixture.ExportService!
                          .PromotionsCsv(promotions
                    .OrderByDescending(x => x.StartDate));

            // Replace the dates and IDs with placeholders
            var dateRegex = new Regex(@"\d{1,2}\.\d{1,2}\.\d{4} г\. \d{1,2}:\d{2}:\d{2}");
            var idRegex = new Regex(@"\b\d+\b");

            var modifiedCsvText = dateRegex.Replace(csvText, "PLACEHOLDER_DATE");
            modifiedCsvText = idRegex.Replace(modifiedCsvText, "PLACEHOLDER_ID");

            // Assert
            

            //Assert.Equal(expectedCsvText, modifiedCsvText);
        }

        [Fact]
        public async Task ExportShippingDiscounts_ShouldBeExecuted()
        {
            fixture.DbContext!.SeedBgCities();

            var res = fixture.DbContext!.ShippingDiscounts!.Add(new ShippingDiscount
            {
                Description = "100% OFF SHIPPING",
                DiscountPercentage = 100,
                MinimumPrice = null,
            });

            await fixture.DbContext!.SaveChangesAsync();

            var shippingDiscounts = await fixture.ShippingDiscountService!
                                 .All();

            // Act
            var csvText = fixture.ExportService!
                          .ShippingDiscountsCsv(shippingDiscounts.ShippingDiscounts
                          .OrderBy(x => x.Description));

            // Replace the dates and IDs with placeholders

        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
