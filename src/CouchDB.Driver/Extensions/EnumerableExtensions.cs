using System;
using System.Collections.Generic;
using System.Linq;

namespace CouchDB.Driver.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether a sequence contains all specified elements by using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence in which to locate a value.</param>
        /// <param name="input">Values to locate in the sequence.</param>
        /// <returns>true if the source sequence contains all elements that has specified values; otherwise, false.</returns>
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
