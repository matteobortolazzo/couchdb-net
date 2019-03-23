using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Client.Exceptions
{
    public class CouchNoIndexException : CouchException
    {
        public CouchNoIndexException(string message, string reason) : base(message, reason) { }
    }
}
