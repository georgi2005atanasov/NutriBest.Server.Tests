namespace NutriBest.Server.Tests.Controllers.Orders.Data
{
    public class OrderTestData
    {
        public static IEnumerable<object[]> GetOrderData()
        {
            yield return new object[] { null!, 1, null!, null!, null!, 20 };
            yield return new object[] { null!, 1, "Finished", null!, null!, 4 };
            yield return new object[] { null!, 1, "Confirmed Paid", null!, null!, 4 };
            yield return new object[] { null!, 1, "Confirmed Paid Finished Shipped", null!, null!, 4 };
            yield return new object[] { null!, 2, "Confirmed Paid", null!, null!, 0 };
            yield return new object[] { null!, 1, "Shipped Finished", null!, null!, 4 };
            yield return new object[] { null!, 2, "Shipped Finished", null!, null!, 0 };
            yield return new object[] { null!, 1, "Confirmed", null!, null!, 13 };
            yield return new object[] { null!, 2, "Confirmed", null!, null!, 0 };
            yield return new object[] { null!, 3, "Paid Shipped", null!, null!, 0 };
            yield return new object[] { null!, 3, null!, null!, null!, 0 };
            yield return new object[] { "TEST USER!!!", 1, null!, null!, null!, 20 };
            yield return new object[] { "1", 1, null!, null!, null!, 1 };
            yield return new object[] { "20", 1, null!, null!, null!, 1 };
            yield return new object[] { null!, 1, null!, null!, DateTime.UtcNow.Subtract(TimeSpan.FromHours(2)), 0 };
            yield return new object[] { null!, 1, null!, DateTime.UtcNow.Subtract(TimeSpan.FromHours(2))!, null!, 20 };
            yield return new object[] { null!, 1, null!, DateTime.UtcNow.Subtract(TimeSpan.FromHours(2))!, DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(40)), 0 };
        }
    }
}
