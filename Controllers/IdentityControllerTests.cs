namespace NutriBest.Server.Tests.Controllers
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Moq;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Identity.Models;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Shared.Responses;
    using Xunit;
    public class IdentityControllerTests : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;

        public IdentityControllerTests(WebApplicationFactory<Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUserNameContainsWhiteSpace()
        {
            var response = await _client.GetAsync("/api/values");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
        }


        //[Fact]
        //public void Register_ShouldReturnBadRequest_WhenUserNameContainsWhiteSpace()
        //{
        //    MyMvc
        //        .Controller<IdentityController>()
        //        .Calling(c => c.Register(new RegisterServiceModel
        //        {
        //            UserName = "test user", // Username with white space
        //            Email = "testuser@example.com",
        //            Password = "Password123!",
        //            ConfirmPassword = "Password123!"
        //        }))
        //        .ShouldReturn()
        //        .BadRequest(result => result
        //            .WithModelOfType<FailResponse>()
        //            .Passing(model => model.Key == "UserName" && model.Message == "Username must not contain white spaces!"));
        
    }
}
