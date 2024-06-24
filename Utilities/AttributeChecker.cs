namespace NutriBest.Server.Tests.Utilities
{
    using System.Reflection;

    public class AttributeChecker
    {
        public static bool HasAttributes(MethodInfo method, params Type[] attributeTypes)
        {
            return attributeTypes.All(attrType => method.GetCustomAttributes(attrType, false).Length > 0);
        }
    }
}
