using CouchDB.Driver.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CouchDB.Driver.Extensions
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Determines whether value is contained in the sequence provided.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="value">The value to locate in the input.</param>
        /// <param name="input">A sequence in which to locate the value.</param>
        /// <returns>true if the input sequence contains an element that has the specified value; otherwise, false.</returns>
        public static bool In<T>(this T value, IEnumerable<T> input)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return input.Contains(value);
        }
        /// <summary>
        /// Determins the field exists in the database. 
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by keySelector.</typeparam>
        /// <param name="source">The value to check.</param>
        /// <param name="fieldName">The name of the field to check.</param>
        /// <returns>true if the field exists; otherwise, false.</returns>
        public static bool FieldExists<T>(this T source, string fieldName)
        {
            return source.GetType().GetProperties().Any(p => p.Name == fieldName);
        }
        /// <summary>
        /// Determins the field is of the specified type. 
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The value to check.</param>
        /// <param name="type">Type couch type to compare.</param>
        /// <returns>true if the field has the specified type; otherwise, false.</returns>
        public static bool IsCouchType<T>(this T source, CouchType couchType)
        {
#pragma warning disable IDE0046 // Convert to conditional expression
            if (couchType == CouchType.CNull && source == null)
            {
                return true;
            }
            if (couchType == CouchType.CBoolean && source is bool)
            {
                return true;
            }
            if (couchType == CouchType.CString && source is string)
            {
                return true;
            }
            if (couchType == CouchType.CNumber && typeof(T).IsPrimitive)
            {
                return true;
            }
            if (couchType == CouchType.CArray && source is IEnumerable)
            {
                return true;
            }
            return couchType == CouchType.CObject && typeof(T).IsClass;
#pragma warning restore IDE0046 // Convert to conditional expression
        }
    }
}
