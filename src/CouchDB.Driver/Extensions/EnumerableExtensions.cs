using System;
using System.Collections.Generic;
using System.Linq;

namespace CouchDB.Driver.Extensions
{
    public static class EnumerableExtensions
    {        
        public static bool Contains<T>(this IEnumerable<T> source, IEnumerable<T> input) where T : IConvertible
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.All(s => source.Contains(s));
        }
        public static bool In<T>(this IEnumerable<T> source, IEnumerable<T> input) where T : IConvertible
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.All(s => source.Contains(s));
        }
    }
}
