using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.Extensions;

internal static class MemberInfoExtensions
{
    public static string GetCouchPropertyName(this MemberInfo memberInfo, JsonNamingPolicy jsonNamingPolicy)
    {
        var jsonPropertyAttributes = memberInfo.GetCustomAttributes(typeof(JsonPropertyNameAttribute), true);
        JsonPropertyNameAttribute? jsonProperty = jsonPropertyAttributes.Length > 0
            ? jsonPropertyAttributes[0] as JsonPropertyNameAttribute
            : null;

        return jsonProperty != null
            ? jsonProperty.Name
            : jsonNamingPolicy.ConvertName(memberInfo.Name);
    }

    extension(List<MethodInfo> queryableMethods)
    {
        public MethodInfo GetMinOrMaxWithoutSelector<T>(string methodName)
            => queryableMethods.Single(mi => mi.Name == methodName
                                             && mi.GetParameters().Length == 1
                                             && mi.GetParameters()[0].ParameterType.GetGenericArguments()[0] ==
                                             typeof(T));

        public MethodInfo GetSumOrAverageWithoutSelector<T>(string methodName)
            => queryableMethods.Single(mi => mi.Name == methodName
                                             && mi.GetParameters().Length == 1
                                             && mi.GetParameters()[0].ParameterType.GetGenericArguments()[0] ==
                                             typeof(T));

        public MethodInfo GetSumOrAverageWithSelector<T>(string methodName)
            => queryableMethods.Single(mi => mi.Name == methodName
                                             && mi.GetParameters().Length == 2
                                             && IsSelector<T>(mi.GetParameters()[1].ParameterType));
    }

    extension(Type type)
    {
        public bool IsExpressionOfFunc(int funcGenericArgs = 2)
            => type.IsGenericType
               && type.GetGenericTypeDefinition() == typeof(Expression<>)
               && type.GetGenericArguments()[0].IsGenericType
               && type.GetGenericArguments()[0].GetGenericArguments().Length == funcGenericArgs;

        public bool IsSelector<T>()
            => type.IsGenericType
               && type.GetGenericTypeDefinition() == typeof(Expression<>)
               && type.GetGenericArguments()[0].IsGenericType
               && type.GetGenericArguments()[0].GetGenericArguments().Length == 2
               && type.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(T);
    }
}