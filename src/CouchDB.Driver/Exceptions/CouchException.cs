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
        internal CouchException(string message, string reason) : base(message, new Exception(reason)) { }
    }
}
