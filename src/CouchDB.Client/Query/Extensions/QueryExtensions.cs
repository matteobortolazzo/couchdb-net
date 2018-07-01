using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace CouchDB.Client.Query.Extensions
{
    internal static class QueryExtensions
    {
        internal static void AddProperty(this ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }

        internal static string GetJsonPropertyName<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
        {
            if (!(propertyLambda.Body is MemberExpression member))
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a method, not a property.");

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(
                    $"Expression '{propertyLambda}' refers to a field, not a property.");
            
            string GetPropertyName(MemberInfo memberInfo)
            {
                var jsonPropertyAttributes = memberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                var jsonProperty = jsonPropertyAttributes.Length > 0 ? jsonPropertyAttributes[0] as JsonPropertyAttribute : null;

                return jsonProperty != null ? jsonProperty.PropertyName : memberInfo.Name;
            }

            var members = new List<string> { GetPropertyName(member.Member) };

            var currentExpression = member.Expression;

            while (currentExpression is MemberExpression cm)
            {
                members.Add(GetPropertyName(cm.Member));
                currentExpression = cm.Expression;
            }

            members.Reverse();
            var propName = string.Join(".", members.ToArray());

            return propName;
        }
    }
}
