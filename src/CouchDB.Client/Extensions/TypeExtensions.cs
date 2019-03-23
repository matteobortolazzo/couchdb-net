using Humanizer;
using Newtonsoft.Json;
using System;

namespace CouchDB.Client.Extensions
{
    public static class TypeExtensions
    {
        public static string GetName(this Type t)
        {
            var jsonObjectAttributes = t.GetCustomAttributes(typeof(JsonObjectAttribute), true);
            var jsonObject = jsonObjectAttributes.Length > 0 ? jsonObjectAttributes[0] as JsonObjectAttribute : null;

            if (jsonObject != null)
                return jsonObject.Id;

            var typeName = t.Name.Camelize();
            return typeName.Pluralize();
        }
    }
}
