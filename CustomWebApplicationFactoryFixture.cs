using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using NutriBest.Server.Data;
using NutriBest.Server.Data.Models;

namespace NutriBest.Server.Tests
{
    public class CustomWebApplicationFactoryFixture : IDisposable
    {
        public CustomWebApplicationFactory<Startup> Factory { get; }

        public CustomWebApplicationFactoryFixture()
        {
            Factory = new CustomWebApplicationFactory<Startup>();
        }

        public void Dispose()
        {
            Factory.Dispose();
        }
    }
}
