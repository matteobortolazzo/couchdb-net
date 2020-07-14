using System;

namespace CouchDB.Driver.Exceptions
{
    public class CouchDeleteException : CouchException
    {
        public CouchDeleteException() : base("Something went wrong.", null, "Something went wrong during the delete operation.") { }

        public CouchDeleteException(string message) : base(message) { }

        public CouchDeleteException(string message, Exception innerException) : base(message, innerException) { }
    }
}
