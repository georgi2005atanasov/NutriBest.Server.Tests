namespace NutriBest.Server.Tests.Controllers.Profile.Data
{
    public class ProfileTestData
    {
        public static IEnumerable<object[]> GetProfileData()
        {
            yield return new object[] { 1, "user", "withOrders", 3 };
            yield return new object[] { 1, null!, "withOrders", 3 };
            yield return new object[] { 1, "UNIQUE_USER_1@example.com", "withoutOrders", 1 };
            yield return new object[] { 1, "UNIQUE_USER", "withoutOrders", 18 };
            yield return new object[] { 1, "UNIQUE_USER", "withOrders", 2 };
            yield return new object[] { 1, "@example.com", "withoutOrders", 20 };
            yield return new object[] { 1, "088", "withOrders", 3 };
            yield return new object[] { 1, "832", "withOrders", 3 };
            yield return new object[] { 2, null!, null!, 0 };
        }
    }
}
