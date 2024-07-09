namespace NutriBest.Server.Tests.Controllers.Products.Data
{
    public static class ProductFilterTestData
    {
        public static IEnumerable<object[]> FilterCombinations()
        {
            yield return new object[] { null!, null!, null!, "Creatines", null!, "500 1000", null!, 1 };
            yield return new object[] { null!, null!, null!, "Creatines and Vitamins", null!, "500 1000", null!, 1 };
            yield return new object[] { null!, null!, null!, "Creatines and Proteins", null!, "500 1000", null!, 2 };
            yield return new object[] { null!, null!, null!, "Creatines and Proteins", null!, null!, null!, 3 };
            yield return new object[] { null!, null!, null!, "Vitamins", null!, null!, "1000 5000", 1 };
            yield return new object[] { null!, null!, null!, null!, null!, null!, "1 5000", 7 };
            yield return new object[] { null!, null!, null!, null!, "pesho", null!, "1 5000", 0 };
            yield return new object[] { null!, null!, null!, null!, "product8", null!, "1 80", 2 };
        }
    }
}
