using System;

namespace CouchDB.Client
{
    public class CouchException : Exception
    {
        public CouchException(string message, string reason) : base(message, new Exception(reason)) { }
    }
}
