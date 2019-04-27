using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CouchDB.Driver.Settings;
using Newtonsoft.Json;

namespace CouchDB.Driver.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static string GetCouchPropertyName(this MemberInfo memberInfo, PropertyCaseType propertyCaseType)
        {
            var jsonPropertyAttributes = memberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
            JsonPropertyAttribute jsonProperty = jsonPropertyAttributes.Length > 0 ? 
                jsonPropertyAttributes[0] as JsonPropertyAttribute : null;

            if (jsonProperty != null)
            {
                return jsonProperty.PropertyName;
            }
            return propertyCaseType.Convert(memberInfo.Name);
        }
    }
}
