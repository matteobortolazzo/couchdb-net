using CouchDB.Driver.Settings;
using Humanizer;
using Newtonsoft.Json;
using System;

namespace CouchDB.Driver.Extensions
{
    public static class TypeExtensions
    {
        internal static string GetName(this Type t, CouchSettings settings)
        {
            var jsonObjectAttributes = t.GetCustomAttributes(typeof(JsonObjectAttribute), true);
            var jsonObject = jsonObjectAttributes.Length > 0 ? jsonObjectAttributes[0] as JsonObjectAttribute : null;

            if (jsonObject != null)
            {
                return jsonObject.Id;
            }
            var typeName = t.Name;
            if (settings.PluralizeEntitis)
            {
                typeName = typeName.Pluralize();
            }
            return settings.DocumentsCaseType.Convert(typeName);
        }
    }
}
