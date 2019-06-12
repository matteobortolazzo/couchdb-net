using CouchDB.Driver.DTOs;
using System;

namespace CouchDB.Driver.Exceptions
{
    /// <summary>
    /// The exception that is thrown when CouchDB return an error.
    /// </summary>
    public class CouchException : Exception
    {
        /// <summary>
        /// Creates a new instance of CouchException.
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="reason">Error reason</param>
        public CouchException(string message, string reason) : this(message, reason, null)
        {
        }

        public CouchException() : this(null, null, null)
        {
        }

        public CouchException(string message) : this(message, null, null)
        {
        }

        public CouchException(string message, Exception innerException) : this(message, null, innerException)
        {
        }

        internal CouchException(CouchError couchError, Exception innerException) : this(couchError?.Error, couchError?.Reason, innerException)
        {
        }

        public CouchException(string message, string reason, Exception innerException) : base(message, innerException)
        {
            Reason = reason;
        }

        public string Reason { get; }
    }
}
