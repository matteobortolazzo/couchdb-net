using System;

namespace CouchDB.Client.Exceptions
{
    public class CouchException : Exception
    {
        public CouchException(string message, string reason) : base(message, new Exception(reason)) { }
    }
}
