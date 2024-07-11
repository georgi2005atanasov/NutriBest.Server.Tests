namespace NutriBest.Server.Tests.InfrastructureTests.ServicesTests
{
    using Xunit;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Data;
    using NutriBest.Server.Data.Models;
    using NutriBest.Server.Infrastructure.Services;
    using NutriBest.Server.Infrastructure.Extensions;

    [Collection("Infrastructure Tests")]
    public class CleanupServicesTests : IAsyncLifetime
    {
        private IServiceProvider? serviceProvider;

        private NutriBestDbContext? db;

        private readonly CustomWebApplicationFactoryFixture fixture;

        private IServiceScope? scope;

        public CleanupServicesTests(CustomWebApplicationFactoryFixture fixture)
        {
            this.fixture = fixture;
            serviceProvider = fixture.Factory.Services.GetService<IServiceProvider>();
        }

        [Fact]
        public async Task PromoCodeCleanupService_ShouldInvalidateExpiredPromoCodes()
        {
            // Arrange
            var service = new PromoCodeCleanupService(serviceProvider!);

            var now = DateTime.UtcNow;

            db!.PromoCodes.AddRange(
                new PromoCode { Id = 1, IsValid = true, Code = "A", Description = "Abc" }, // Should be invalidated
                new PromoCode { Id = 2, IsValid = true, Code = "B", Description = "Bbc" },  // Should remain valid
                new PromoCode { Id = 3, IsValid = true, Code = "C", Description = "Cbc" }  // Should be invalidated
            );

            await db.SaveChangesAsync();

            foreach (var promoCode in db.PromoCodes)
            {
                if (promoCode.Code == "A")
                {
                    promoCode.CreatedOn = promoCode.CreatedOn.AddDays(-100);
                }
            }

            await db.SaveChangesAsync();

            // Act
            await service.StartAsync(CancellationToken.None);
            await Task.Delay(1000);

            // Assert
            var invalidPromoCodes = db!
                .PromoCodes
                .IgnoreQueryFilters()
                .Where(p => !p.IsValid)
                .ToList();

            Assert.Single(invalidPromoCodes);
        }

        [Fact]
        public async Task PromotionActivationService_ShouldActivateEligiblePromotions()
        {
            // Arrange
            var service = new PromotionActivationService(serviceProvider!);

            var now = DateTime.UtcNow;

            db!.Promotions.AddRange(
                new Promotion { PromotionId = 1, DiscountPercentage = 20, Description = "PROMO1", StartDate = now.AddHours(-1), IsActive = false }, // Should be activated
                new Promotion { PromotionId = 2, DiscountPercentage = 20, Description = "PROMO2", StartDate = now.AddHours(1), IsActive = false },  // Should remain inactive
                new Promotion { PromotionId = 3, DiscountPercentage = 20, Description = "PROMO3", StartDate = now.AddHours(-2), IsActive = false }  // Should be activated
            );

            await db.SaveChangesAsync();

            // Act
            await service.StartAsync(CancellationToken.None);

            // Give some time for the background service to run
            await Task.Delay(1000);

            // Assert
            var activePromotions = db!
                .Promotions
                .Where(p => p.IsActive)
                .ToList();

            Assert.Equal(2, activePromotions.Count); // Two promotions should be activated
            Assert.Contains(activePromotions, p => p.PromotionId == 1);
            Assert.Contains(activePromotions, p => p.PromotionId == 3);
        }

        [Fact]
        public async Task PromotionCleanupService_ShouldDeactivateAndRemoveExpiredPromotions()
        {
            // Arrange
            var service = new PromotionCleanupService(serviceProvider!);

            var now = DateTime.UtcNow;

            db!.Promotions.AddRange(
                new Promotion { PromotionId = 1, DiscountPercentage = 20, Description = "PROMO1", StartDate = now, IsActive = true },
                new Promotion { PromotionId = 2, DiscountPercentage = 20, Description = "PROMO2", StartDate = now, IsActive = true },
                new Promotion { PromotionId = 3, DiscountPercentage = 25, Description = "PROMO3", StartDate = now, IsActive = true }
            );

            await db.SaveChangesAsync();

            foreach (var promo in db.Promotions)
            {
                if (promo.DiscountPercentage == 20)
                {
                    promo.EndDate = now.AddHours(-1);
                }
            }

            await db.SaveChangesAsync();

            // Act
            await service.StartAsync(CancellationToken.None);

            // Give some time for the background service to run
            await Task.Delay(1000);

            // Assert
            var activePromotions = db!.Promotions
                .Where(p => p.IsActive)
                .ToList();
            var removedPromotions = db.Promotions
                .IgnoreQueryFilters()
                .Where(p => !p.IsActive)
                .ToList();

            Assert.Single(activePromotions);
            Assert.Contains(activePromotions, p => p.PromotionId == 3);
            Assert.Equal(2, removedPromotions.Count);
            Assert.Contains(removedPromotions, p => p.PromotionId == 1);
            Assert.Contains(removedPromotions, p => p.PromotionId == 2);
        }

        [Fact]
        public async Task ShippingDiscountCleanupService_ShouldCleanupExpiredShippingDiscounts()
        {
            // Arrange
            var service = new ShippingDiscountCleanupService(serviceProvider!);

            var now = DateTime.UtcNow;

            var shippingDiscount1 = new ShippingDiscount { Id = 1, DiscountPercentage = 20, Description = "20% off 1", EndDate = now.AddHours(-1) };
            var shippingDiscount2 = new ShippingDiscount { Id = 2, DiscountPercentage = 20, Description = "20% off 2", EndDate = now.AddHours(1) };

            db!.ShippingDiscounts.AddRange(shippingDiscount1, shippingDiscount2);

            db.Countries.AddRange(
                new Country { CountryName = "Country1", ShippingDiscountId = 1 },
                new Country { CountryName = "Country2", ShippingDiscountId = 2 }
            );

            await db.SaveChangesAsync();

            // Act
            await service.StartAsync(CancellationToken.None);

            // Give some time for the background service to run
            await Task.Delay(1000);

            // Assert
            var activeShippingDiscounts = db!
                .ShippingDiscounts
                .Where(sd => !sd.IsDeleted)
                .ToList();

            var countriesWithNullShippingDiscount = db
                .Countries
                .Where(c => c.ShippingDiscountId == null)
                .ToList();

            Assert.Single(activeShippingDiscounts);
            Assert.Single(countriesWithNullShippingDiscount);
            Assert.Contains(activeShippingDiscounts, sd => sd.Id == 2); 
            Assert.Contains(countriesWithNullShippingDiscount, c => c.Id == 1);
        }

        public async Task InitializeAsync()
        {
            await fixture!.ResetDatabaseAsync();
            scope = fixture.Factory.Services.CreateScope();
            db = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}