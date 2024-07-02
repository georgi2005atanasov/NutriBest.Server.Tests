namespace NutriBest.Server.Tests.Extensions
{
    using System.Net.Http.Json;

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, string requestUri, T value)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri)
            {
                Content = JsonContent.Create(value)
            };

            return await client.SendAsync(request);
        }
    }
}
