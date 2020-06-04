using CouchDB.Driver.DTOs;
using System;

namespace CouchDB.Driver.Exceptions
{
    /// <summary>
    /// The exception that is thrown when CouchDB return an error.
    /// </summary>
    public class CouchException : Exception
    {
        public string? Reason { get; }

        public CouchException() { }

        public CouchException(string message) : base(message) { }

        public CouchException(string message, Exception? innerException) : base(message, innerException) { }

        public CouchException(string message, Exception? innerException, string reason) : base(message, innerException)
        {
            Reason = reason;
        }

        internal CouchException(CouchError couchError, Exception innerException) : this(couchError.Error, innerException, couchError.Reason) { }
    }
}
