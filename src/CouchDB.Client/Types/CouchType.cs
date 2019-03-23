using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Client.Types
{
    public class CouchType
    {
        public string Value { get; }

        private CouchType(string value)
        {
            Value = value;
        }

        public static CouchType Null = new CouchType("null");
        public static CouchType Boolean = new CouchType("boolean");
        public static CouchType Number = new CouchType("number");
        public static CouchType String = new CouchType("string");
        public static CouchType Array = new CouchType("array");
        public static CouchType Object = new CouchType("object");
    }
}
