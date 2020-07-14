using CouchDB.Driver.DTOs;
using System;

namespace CouchDB.Driver.Exceptions
{
    /// <summary>
    /// The exception that is thrown when something is not found.
    /// </summary>
    public class CouchNotFoundException : CouchException
    {
        internal CouchNotFoundException(CouchError couchError, Exception innerException) : base(couchError, innerException) { }

        public CouchNotFoundException() { }

        public CouchNotFoundException(string message) : base(message) { }

        public CouchNotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }
}
