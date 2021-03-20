using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CouchDB.Driver.Options;
using Newtonsoft.Json;

namespace CouchDB.Driver.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static string GetCouchPropertyName(this MemberInfo memberInfo, PropertyCaseType propertyCaseType)
        {
            object[] jsonPropertyAttributes = memberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
            JsonPropertyAttribute? jsonProperty = jsonPropertyAttributes.Length > 0 
                ? jsonPropertyAttributes[0] as JsonPropertyAttribute
                : null;

            return jsonProperty != null
                ? jsonProperty.PropertyName
                : propertyCaseType.Convert(memberInfo.Name);
        }

        public static MethodInfo GetMinOrMaxWithoutSelector<T>(this List<MethodInfo> queryableMethods, string methodName)
            => queryableMethods.Single(
                mi => mi.Name == methodName
                      && mi.GetParameters().Length == 1
                      && mi.GetParameters()[0].ParameterType.GetGenericArguments()[0] == typeof(T));

        public static MethodInfo GetSumOrAverageWithoutSelector<T>(this List<MethodInfo> queryableMethods, string methodName)
            => queryableMethods.Single(
                mi => mi.Name == methodName
                      && mi.GetParameters().Length == 1
                      && mi.GetParameters()[0].ParameterType.GetGenericArguments()[0] == typeof(T));

        public static MethodInfo GetSumOrAverageWithSelector<T>(this List<MethodInfo> queryableMethods, string methodName)
            => queryableMethods.Single(
                mi => mi.Name == methodName
                      && mi.GetParameters().Length == 2
                      && IsSelector<T>(mi.GetParameters()[1].ParameterType));

        public static bool IsExpressionOfFunc(this Type type, int funcGenericArgs = 2)
            => type.IsGenericType
               && type.GetGenericTypeDefinition() == typeof(Expression<>)
               && type.GetGenericArguments()[0].IsGenericType
               && type.GetGenericArguments()[0].GetGenericArguments().Length == funcGenericArgs;

        public static bool IsSelector<T>(this Type type)
            => type.IsGenericType
               && type.GetGenericTypeDefinition() == typeof(Expression<>)
               && type.GetGenericArguments()[0].IsGenericType
               && type.GetGenericArguments()[0].GetGenericArguments().Length == 2
               && type.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(T);
    }
}
