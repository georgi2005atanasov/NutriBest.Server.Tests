namespace NutriBest.Server.Tests.Mocks
{
    using Microsoft.EntityFrameworkCore;
    using Moq;

    public static class MockDbSet
    {
        public static Mock<DbSet<T>> Create<T>(IEnumerable<T> elements) where T : class
        {
            var queryable = elements.AsQueryable();
            var dbSetMock = new Mock<DbSet<T>>();

            dbSetMock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSetMock.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return dbSetMock;
        }
    }
}
