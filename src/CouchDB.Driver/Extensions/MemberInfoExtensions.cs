using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Settings;
using Newtonsoft.Json;

namespace CouchDB.Driver.Extensions
{
    internal static class MemberInfoExtensions
    {
        public static string GetCouchPropertyName(this MemberInfo memberInfo, PropertyCaseType propertyCaseType)
        {
            var jsonPropertyAttributes = memberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
            JsonPropertyAttribute? jsonProperty = jsonPropertyAttributes.Length > 0 
                ? jsonPropertyAttributes[0] as JsonPropertyAttribute
                : null;

            return jsonProperty != null
                ? jsonProperty.PropertyName
                : propertyCaseType.Convert(memberInfo.Name);
        }

        public static MethodInfo GetEnumerableMethod(this MethodInfo queryableMethodInfo, Type sourceType)
        {
            Check.NotNull(queryableMethodInfo, nameof(queryableMethodInfo));

            MethodInfo FindEnumerableMethod()
            {
                if (queryableMethodInfo.Name == nameof(Queryable.Max) || queryableMethodInfo.Name == nameof(Queryable.Min))
                {
                    return queryableMethodInfo.FindEnumerableNumeric(sourceType);
                }

                return sourceType.GetMethods().Single(info =>
                    queryableMethodInfo.Name == info.Name && ReflectionComparator.IsCompatible(queryableMethodInfo, info));
            }

            MethodInfo genericEnumerableMethodInfo = FindEnumerableMethod();

            Type[] requestedGenericParameters = genericEnumerableMethodInfo.GetGenericMethodDefinition().GetGenericArguments();
            Type[] genericParameters = queryableMethodInfo.GetGenericArguments();
            Type[] usableParameters = genericParameters.Take(requestedGenericParameters.Length).ToArray();
            MethodInfo enumerableMethodInfo = genericEnumerableMethodInfo.MakeGenericMethod(usableParameters);

            return enumerableMethodInfo;
        }

        private static MethodInfo FindEnumerableNumeric(this MethodBase queryableMethodInfo, Type sourceType)
        {
            Type[] genericParams = queryableMethodInfo.GetGenericArguments();
            return sourceType.GetMethods().Single(enumerableMethodInfo =>
            {
                Type[] enumerableArguments = enumerableMethodInfo.GetGenericArguments();
                return
                    enumerableMethodInfo.Name == queryableMethodInfo.Name &&
                    enumerableArguments.Length == genericParams.Length - 1 &&
                    enumerableMethodInfo.ReturnType == genericParams[1];
            });
        }
    }

    internal static class MethodInfoHelper
    {
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
