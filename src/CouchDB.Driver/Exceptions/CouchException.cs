using System;

namespace CouchDB.Driver.Exceptions
{
    public class CouchException : Exception
    {
        public CouchException(string message, string reason) : base(message, new Exception(reason)) { }
    }
}
