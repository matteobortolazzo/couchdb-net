using CouchDB.Client.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Client.Extensions
{
    public static class ObjectExtensions
    {
        public static bool FieldExists(this object obj, bool doExists = true)
        {
            return true;
        }
        public static bool IsCouchType(this object obj, CouchType type)
        {
            return true;
        }
    }
}
