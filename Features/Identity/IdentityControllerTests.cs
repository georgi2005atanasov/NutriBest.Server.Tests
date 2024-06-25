namespace NutriBest.Server.Tests.Features.Identity
{
    using Microsoft.AspNetCore.Mvc.Testing;
    using Newtonsoft.Json;
    using NutriBest.Server.Features;
    using NutriBest.Server.Features.Identity;
    using System.Text;

    public class IdentityControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {

        private readonly WebApplicationFactory<Program> _factory;

        public IdentityControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        //[Theory]
        //[InlineData("/Products")]
        //public async Task TestLoginEndpoint(string url)
        //{
        //    // Arrange
        //    var client = _factory.CreateClient();

        //    // Act
        //    var response = await client.GetAsync(url);

        //    // Assert
        //    response.EnsureSuccessStatusCode(); // Status Code 200-299
        //    Assert.Equal("text/html; charset=utf-8",
        //        response.Content.Headers.ContentType.ToString());
        //}
    }
}
