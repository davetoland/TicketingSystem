using System.Collections.Generic;
using System.Linq;
using Aareon.Data.Entities;
using MockQueryable.Moq;

namespace Aareon.Business.Tests
{
    public static class Extensions
    {
        public static IQueryable<T> ToQueryable<T>(this IEnumerable<T> collection) where T: class
        {
            // https://github.com/romantitov/MockQueryable
            return collection.AsQueryable().BuildMock().Object;
        }
        
        public static IQueryable<T> ToQueryable<T>(this T t) where T: DbEntity
        {
            // https://github.com/romantitov/MockQueryable
            return new List<T>{t}.AsQueryable().BuildMock().Object;
        }
    }
}