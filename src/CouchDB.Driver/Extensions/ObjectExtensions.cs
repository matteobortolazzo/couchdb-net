using CouchDB.Driver.Types;

namespace CouchDB.Driver.Extensions
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
