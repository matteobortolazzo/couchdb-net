using CouchDB.Driver.Types;

namespace CouchDB.Driver.Extensions
{
    public static class ObjectExtensions
    {
        public static bool FieldExists<T>(this T obj, bool doExists)
        {
            return true;
        }
        public static bool IsCouchType<T>(this T obj, CouchType type)
        {
            return true;
        }
    }
}
