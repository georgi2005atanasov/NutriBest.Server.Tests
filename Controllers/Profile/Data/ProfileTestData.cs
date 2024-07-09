namespace NutriBest.Server.Tests.Controllers.Profile.Data
{
    public class ProfileTestData
    {
        public static IEnumerable<object[]> GetProfileData()
        {
            yield return new object[] { 1, "user", "withOrders", 1 };
            yield return new object[] { 1, null!, "withOrders", 1 };
            yield return new object[] { 1, "UNIQUE_USER_1@example.com", "withoutOrders", 1 };
            yield return new object[] { 1, "UNIQUE_USER", "withoutOrders", 20 };
            yield return new object[] { 1, "UNIQUE_USER", "withOrders", 0 };
            yield return new object[] { 1, "@example.com", "withoutOrders", 22 };
            yield return new object[] { 2, null!, null!, 0 };
        }
    }
}
