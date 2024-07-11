namespace NutriBest.Server.Tests.Fixtures
{
    using System.Reflection;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using AutoMapper;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Admin;
    using NutriBest.Server.Features.Export;
    using NutriBest.Server.Features.Brands;
    using NutriBest.Server.Features.Reports;
    using NutriBest.Server.Features.Products;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Promotions;
    using NutriBest.Server.Tests.Fixtures.Base;
    using NutriBest.Server.Infrastructure.Services;
    using NutriBest.Server.Features.Images;
    using NutriBest.Server.Tests.Fixtures;
    using NutriBest.Server.Features.Newsletter;
    using NutriBest.Server.Features.Flavours;
    using NutriBest.Server.Features.Categories;
    using NutriBest.Server.Features.Orders;
    using NutriBest.Server.Features.Packages;
    using NutriBest.Server.Features.PromoCodes;
    using NutriBest.Server.Features.ShippingDiscounts;

    public class ReportFixture : BaseFixture
    {
        public IConfiguration Configuration { get; private set; }

        public IExportService? ExportService { get; private set; }

        public IBrandService? BrandService { get; private set; }

        public ICategoryService? CategoryService { get; private set; }

        public IFlavourService? FlavourService { get; private set; }

        public IOrderService? OrderService { get; private set; }

        public IPackageService? PackageService { get; private set; }

        public IProductService? ProductService { get; private set; }

        public IProfileService? ProfileService { get; private set; }

        public IPromoCodeService? PromoCodeService { get; private set; }

        public IPromotionService? PromotionService { get; private set; }

        public IReportService? ReportService { get; private set; }

        public IShippingDiscountService? ShippingDiscountService { get; private set; }

        public ReportFixture()
            : base()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Tests.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            InitializeServices();
        }

        public override void InitializeServices()
        {
            base.InitializeServices();

            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddMaps(AppDomain.CurrentDomain.GetAssemblies());
            });

            mapperConfiguration.AssertConfigurationIsValid();
            var mapper = mapperConfiguration.CreateMapper();

            var dbField = typeof(IdentityService)
                .GetField("db", BindingFlags.NonPublic | BindingFlags.Instance);

            if (dbField == null)
            {
                throw new InvalidOperationException("The db field was not found in the IdentityService.");
            }

            var dbContext = (NutriBestDbContext)dbField.GetValue(IdentityService)!;

            ExportService = new ExportService();
            BrandService = new BrandService(DbContext!, new Mock<IImageService>().Object);
            CategoryService = new CategoryService(DbContext!);
            FlavourService = new FlavourService(DbContext!);
            OrderService = new OrderService(DbContext!,
                CurrentUserServiceMock.Object,
                Configuration,
                NotificationServiceMock!.Object,
                mapper);
            PackageService = new PackageService(DbContext!);
            PromotionService = new PromotionService(DbContext!,
                CategoryService,
                mapper);
            ProductService = new ProductService(DbContext!,
                PromotionService,
                CurrentUserServiceMock.Object,
                mapper);
            ProfileService = new ProfileService(CurrentUserServiceMock.Object,
                dbContext,
                UserManager!);
            PromoCodeService = new PromoCodeService(DbContext!);
            ReportService = new ReportService(DbContext!);
            ShippingDiscountService = new ShippingDiscountService(DbContext!);
        }
    }
}
