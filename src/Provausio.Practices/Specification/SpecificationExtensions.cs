using System.Collections.Generic;
using System.Linq;
using Provausio.Practices.Specification.Provausio.Practices.Patterns;

namespace Provausio.Practices.Specification
{
    public static class SpecificationExtensions
    {
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.Where(specification.IsSatisfiedBy);
        }

        public static T First<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.First(specification.IsSatisfiedBy);
        }

        public static T FirstOrDefault<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.FirstOrDefault(specification.IsSatisfiedBy);
        }

        public static T Last<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.Last(specification.IsSatisfiedBy);
        }

        public static T LastOrDefault<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.LastOrDefault(specification.IsSatisfiedBy);
        }

        public static T Single<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.Single(specification.IsSatisfiedBy);
        }

        public static T SingleOrDefault<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.SingleOrDefault(specification.IsSatisfiedBy);
        }

        public static bool Any<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.Any(specification.IsSatisfiedBy);
        }

        public static bool All<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.All(specification.IsSatisfiedBy);
        }

        public static int Count<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.Count(specification.IsSatisfiedBy);
        }

        public static long LongCount<T>(this IEnumerable<T> source, Specification<T> specification)
        {
            return source.LongCount(specification.IsSatisfiedBy);
        }
    }
}
