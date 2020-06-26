using System;

namespace CouchDB.Driver.Helpers
{
    internal static class Check
    {
        public static void NotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }
    }
}
