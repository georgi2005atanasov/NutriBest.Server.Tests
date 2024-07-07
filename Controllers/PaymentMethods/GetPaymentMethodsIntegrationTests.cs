namespace NutriBest.Server.Tests.Controllers.PaymentMethods
{
    using System.Text;
    using System.Text.Json;
    using Xunit;

    [Collection("Payment Methods Controller Tests")]
    public class GetPaymentMethodsIntegrationTests
    {
        private ClientHelper clientHelper;

        public GetPaymentMethodsIntegrationTests(CustomWebApplicationFactoryFixture fixture) 
            => clientHelper = new ClientHelper(fixture);

        [Fact]
        public async Task GetPaymentMethods_ShouldBeExecuted()
        {
            var client = await clientHelper.GetAdministratorClientAsync();

            var response = await client.GetAsync("/PaymentMethods");
            var data = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<string[]>(data) ?? new string[2] { "", "" };

            Assert.Equal(2, result.Length);
            Assert.Contains("CashOnDelivery", result);
            Assert.Contains("BankTransfer", result);
        }
    }
}
