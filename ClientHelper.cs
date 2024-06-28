namespace NutriBest.Server.Tests
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NutriBest.Server.Features.Identity.Models;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    public class ClientHelper
    {
        private CustomWebApplicationFactoryFixture fixture;

        private IConfiguration config;

        public ClientHelper(CustomWebApplicationFactoryFixture fixture)
        {
            this.fixture = fixture;
            var scope = fixture.Factory.Services.CreateScope();
            config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        }

        public HttpClient GetAnonymousClient()
        {
            return fixture.Factory.CreateClient();
        }

        public async Task<HttpClient> GetEmployeeClientAsync()
        {
            return await GetAuthenticatedClientAsync("employee", "Password123!");
        }

        public async Task<HttpClient> GetOtherUserClientAsync()
        {
            return await GetAuthenticatedClientAsync("user", "Password123!");
        }

        public async Task<HttpClient> GetAdministratorClientAsync()
        {
            return await GetAuthenticatedClientAsync(config.GetValue<string>("Admin:UserName"), config.GetValue<string>("Admin:Password"));
        }

        public async Task<HttpClient> GetAuthenticatedClientAsync(string username, string password)
        {
            var client = fixture.Factory.CreateClient();
            var token = await GetJwtTokenAsync(client, username, password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private async Task<string> GetJwtTokenAsync(HttpClient client, string username, string password)
        {
            var loginModel = new LoginServiceModel { UserName = username, Password = password };
            var response = await client.PostAsJsonAsync("/Identity/Login", loginModel);
            response.EnsureSuccessStatusCode();
            var token = await response.Content.ReadAsStringAsync();
            return token ?? "";
        }
    }

}
