namespace NutriBest.Server.Tests.Controllers
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.IdentityModel.Tokens;
    using Moq;
    using NutriBest.Server.Data;
    using NutriBest.Server.Features.Identity;
    using NutriBest.Server.Features.Identity.Models;
    using NutriBest.Server.Features.Notifications;
    using NutriBest.Server.Shared.Responses;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http.Json;
    using System.Security.Claims;
    using System.Text;
    using Xunit;
    public class IdentityControllerTests : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly ClientHelper clientHelper;
        private readonly CustomWebApplicationFactory<Startup> factory;
        private readonly ApplicationSettings appSettings;

        public IdentityControllerTests(CustomWebApplicationFactory<Startup> factory)
        {
            clientHelper = new ClientHelper(factory);
            this.factory = factory;
            using (var scope = factory.Services.CreateScope())
            {
                var scopedServices = scope.ServiceProvider;
                //_userManager = scopedServices.GetRequiredService<UserManager<User>>();
                var options = scopedServices.GetRequiredService<IOptions<ApplicationSettings>>();
                appSettings = options.Value;
            }
        }


        [Fact]
        public async Task LoginEndpoint_ShouldReturnValidToken()
        {
            using (var scope = factory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<NutriBestDbContext>();

                // Seed additional data if necessary
                //dbContext..Add(new YourEntity { /* properties */ });
                //dbContext.SaveChanges();
            }

            var loginModel = new //LoginServiceModel
            {
                UserName = "user",
                Password = "Password123!"
            };

            var client = clientHelper.GetAnonymousClientAsync();

            // Act
            var response = await client.PostAsJsonAsync("/Identity/Login", loginModel);
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadAsStringAsync();
            ValidateToken(token);
        }

        private void ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

            Assert.NotNull(principal);
            Assert.IsType<JwtSecurityToken>(validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;
            var claimToCheck = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
            Assert.Equal("User", claimToCheck.Value);
        }
    }
}
