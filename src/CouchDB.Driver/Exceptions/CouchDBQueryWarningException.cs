using System;

namespace CouchDB.Driver.Exceptions
{
    /// <summary>
    /// The exception that is thrown if the query returns a warning.
    /// </summary>
    public class CouchDBQueryWarningException : Exception
    {
        public CouchDBQueryWarningException(string message) : base(message) { }
    }
}
