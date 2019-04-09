using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Driver.Exceptions
{
    public class CouchDeleteException : CouchException
    {
        public CouchDeleteException() : base("Something went wrong.", "Something went wrong during the delete operation.") { }

        public CouchDeleteException(string message) : base(message)
        {
        }

        public CouchDeleteException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
