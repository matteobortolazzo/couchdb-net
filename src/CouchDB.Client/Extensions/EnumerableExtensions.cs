using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CouchDB.Client.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool ContainsAll<T>(this IEnumerable<T> source, IEnumerable<T> input)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return source.Equals(input);
        }
        public static bool ContainsNone<T>(this IEnumerable<T> source, IEnumerable<T> input)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return !source.Equals(input);
        }
        public static bool In<T>(this IEnumerable<T> source, IEnumerable<T> input) where T : struct
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.All(s => source.Contains(s));
        }
        public static bool NotIn<T>(this IEnumerable<T> source, IEnumerable<T> input) where T : struct
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.All(s => !source.Contains(s));
        }
    }
}
