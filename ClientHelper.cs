namespace NutriBest.Server.Tests
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;

    public class ClientHelper
    {
        private readonly CustomWebApplicationFactory<Startup> factory;

        public ClientHelper(CustomWebApplicationFactory<Startup> factory)
        {
            this.factory = factory;
        }

        public HttpClient GetAnonymousClientAsync()
        {
            return factory.CreateClient();
        }

        public async Task<HttpClient> GetEmployeeClientAsync()
        {
            return await GetAuthenticatedClientAsync("employee", "password");
        }

        public async Task<HttpClient> GetOtherUserClientAsync()
        {
            return await GetAuthenticatedClientAsync("otheruser", "password");
        }

        public async Task<HttpClient> GetAdministratorClientAsync()
        {
            return await GetAuthenticatedClientAsync("admin", "password");
        }

        private async Task<HttpClient> GetAuthenticatedClientAsync(string username, string password)
        {
            var client = factory.CreateClient();
            var token = await GetJwtTokenAsync(client, username, password);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private async Task<string> GetJwtTokenAsync(HttpClient client, string username, string password)
        {
            var loginModel = new /*LogiModel*/ { Username = username, Password = password };
            var response = await client.PostAsJsonAsync("/api/auth/login", loginModel);
            response.EnsureSuccessStatusCode();
            //var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return "";
            //return loginResponse.Token;
        }
    }

}
