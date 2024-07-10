namespace NutriBest.Server.Tests.Controllers.Newsletter.Data
{
    public class SubscriberTestData
    {
        public static IEnumerable<object[]> GetSubscriberData()
        {
            yield return new object[] { 1, "", "all", 11 };
            yield return new object[] { 1, "", "withoutOrders", 7 };
            yield return new object[] { 1, null!, "withOrders", 4 };
            yield return new object[] { 1, "john", "withOrders", 0 };
            yield return new object[] { 1, "jane", "withoutOrders", 0 };
            yield return new object[] { 1, "088", "withOrders", 4 };
            yield return new object[] { 1, "832", "withOrders", 4 };
        }
    }
}
