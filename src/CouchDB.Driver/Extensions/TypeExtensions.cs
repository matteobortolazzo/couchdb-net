using CouchDB.Driver.Types;
using Humanizer;
using Newtonsoft.Json;
using System;

namespace CouchDB.Driver.Extensions
{
    public static class TypeExtensions
    {
        public static string GetName(this Type t, CouchSettings settings)
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
            if (settings.EntityCaseType != CaseType.None)
            {
                return settings.EntityCaseType.Convert(typeName);
            }
            return typeName;
        }
    }
}
