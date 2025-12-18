using CouchDB.Driver.Types;

namespace CouchDB.Driver.Query.Extensions;

public static class ObjectQueryExtensions
{
    /// <param name="value">The value to locate in the input.</param>
    /// <typeparam name="T">The type of the elements of source.</typeparam>
    extension<T>(T value)
    {
        /// <summary>
        /// Determines whether value is contained in the sequence provided.
        /// </summary>
        /// <param name="input">A sequence in which to locate the value.</param>
        /// <returns>true if the input sequence contains an element that has the specified value; otherwise, false.</returns>
        public bool In(IEnumerable<T> input)
        {
            ArgumentNullException.ThrowIfNull(input);

            return input.Contains(value);
        }

        /// <summary>
        /// Determines the field exists in the database. 
        /// </summary>
        /// <param name="fieldName">The name of the field to check.</param>
        /// <returns>true if the field exists; otherwise, false.</returns>
        public bool FieldExists(string fieldName)
        {
            ArgumentNullException.ThrowIfNull(value);

            return value.GetType().GetProperties().Any(p => p.Name == fieldName);
        }

        /// <summary>
        /// Determines the field is of the specified type. 
        /// </summary>
        /// <param name="couchType">Type couch type to compare.</param>
        /// <returns>true if the field has the specified type; otherwise, false.</returns>
        public bool IsCouchType(CouchType couchType)
        {
            if (couchType == CouchType.CNull && value == null)
            {
                return true;
            }

            if (couchType == CouchType.CBoolean && value is bool)
            {
                return true;
            }

            if (couchType == CouchType.CString && value is string)
            {
                return true;
            }

            if (couchType == CouchType.CNumber && typeof(T).IsPrimitive)
            {
                return true;
            }

            if (couchType == CouchType.CArray && value is IEnumerable)
            {
                return true;
            }

            return couchType == CouchType.CObject && typeof(T).IsClass;
        }
    }
}