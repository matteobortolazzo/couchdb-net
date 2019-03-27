using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CouchDB.Driver.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Determins whether value is contained in the sequence provided.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="value">The value to locate in the input.</param>
        /// <param name="input">A sequence in which to locate the value.</param>
        /// <returns>true if the input sequence contains an element that has the specified value; otherwise, false.</returns>
        public static bool In<T>(this T value, IEnumerable<T> input) where T : IConvertible
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.Contains(value);
        }
        /// <summary>
        /// Determins the field exists in the database. 
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="doExists">Whether if the field must exists or not.</param>
        /// <returns>true if the field exists; otherwise, false.</returns>
        public static bool FieldExists<T>(this T value, bool doExists)
        {
            return true;
        }
        /// <summary>
        /// Determins the field is of the specified type. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="type">Type couch type to compare.</param>
        /// <returns>true if the field has the specified type; otherwise, false.</returns>
        public static bool IsCouchType<T>(this T value, CouchType type)
        {
            return true;
        }
    }
}
