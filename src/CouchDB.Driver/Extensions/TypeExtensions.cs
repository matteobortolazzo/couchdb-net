using Humanizer;
using Newtonsoft.Json;
using System;

namespace CouchDB.Driver.Extensions
{
    public static class TypeExtensions
    {
        public static string GetName(this Type t, bool pluralize)
        {
            var jsonObjectAttributes = t.GetCustomAttributes(typeof(JsonObjectAttribute), true);
            var jsonObject = jsonObjectAttributes.Length > 0 ? jsonObjectAttributes[0] as JsonObjectAttribute : null;

            if (jsonObject != null)
            {
                return jsonObject.Id;
            }
            var typeName = t.Name.Camelize();
            if (pluralize)
            {
                return typeName.Pluralize();
            }
            return typeName;
        }
    }
}
