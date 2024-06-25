namespace NutriBest.Server.Tests
{
    using MyTested.AspNetCore.Mvc;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;

    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration) 
            : base(configuration)
        {
        }

        public void ConfigureTestServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            // Replace only your own custom services. The ASP.NET Core ones 
            // are already replaced by MyTested.AspNetCore.Mvc.
        }
    }
}
