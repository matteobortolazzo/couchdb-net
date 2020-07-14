using System.Collections.Generic;
using System.Linq;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.Query.Extensions
{
    public static class EnumerableQueryExtensions
    {
        /// <summary>
        /// Determines whether a sequence contains all specified elements by using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence in which to locate a value.</param>
        /// <param name="input">Values to locate in the sequence.</param>
        /// <returns>true if the source sequence contains all elements that has specified values; otherwise, false.</returns>
        public static bool Contains<T>(this IEnumerable<T> source, IEnumerable<T> input)
        {
            Check.NotNull(source, nameof(source));
            Check.NotNull(input, nameof(input));

            return input.All(source.Contains!);
        }
    }
}
