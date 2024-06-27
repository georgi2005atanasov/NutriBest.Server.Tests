namespace NutriBest.Server.Tests.Controllers.Identity.Data
{
    using System.Collections.Generic;

    public class InvalidIdentityData
    {
        public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { "user", "user@example.com", "Pesho12345", "Pesho123456" },
            new object[] { "us er", "user@example.com", "Pesho12345", "Pesho12345" },
            new object[] { "user", "user@example.com", "Pesho123", "Pesho123" },
            new object[] { "user", "userexample.com", "Pesho12345", "Pesho12345" },
            new object[] { "user", "user example.com", "Pesho12345", "Pesho12345" },
            new object[] { "user", "user example.com", "Pesho12345", "Pesho12345" },
            new object[] { "us er", "user example.com", "Pesho125", "Pesho12345" },
            new object[] { "us er", "user example.com", "", "" },
        };
    }
}
