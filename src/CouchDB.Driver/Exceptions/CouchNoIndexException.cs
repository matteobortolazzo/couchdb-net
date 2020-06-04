using CouchDB.Driver.DTOs;
using System;

namespace CouchDB.Driver.Exceptions
{
    /// <summary>
    /// The exception that is thrown when there is no index for the query.
    /// </summary>
    public class CouchNoIndexException : CouchException
    {
        internal CouchNoIndexException(CouchError couchError, Exception innerException) : base(couchError, innerException) { }

        public CouchNoIndexException() { }

        public CouchNoIndexException(string message) : base(message) { }
        public CouchNoIndexException(string message, Exception innerException) : base(message, innerException) { }
    }
}
