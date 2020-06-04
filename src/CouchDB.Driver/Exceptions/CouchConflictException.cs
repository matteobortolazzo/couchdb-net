using CouchDB.Driver.DTOs;
using System;

namespace CouchDB.Driver.Exceptions
{
    /// <summary>
    /// The exception that is thrown when there is a conflict.
    /// </summary>
    public class CouchConflictException : CouchException
    {
        internal CouchConflictException(CouchError couchError, Exception innerException) : base(couchError, innerException) { }

        public CouchConflictException() { }

        public CouchConflictException(string message) : base(message) { }

        public CouchConflictException(string message, Exception innerException) : base(message, innerException) { }
    }
}
