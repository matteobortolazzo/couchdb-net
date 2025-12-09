using Humanizer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Options;

namespace CouchDB.Driver.Extensions;

internal static class TypeExtensions
{
    extension(Type type)
    {
        public string GetName(CouchOptions options)
        {
            var jsonObjectAttributes = type.GetCustomAttributes(typeof(DatabaseNameAttribute), true);
            DatabaseNameAttribute? jsonObject = jsonObjectAttributes.Length > 0
                ? jsonObjectAttributes[0] as DatabaseNameAttribute
                : null;

            if (jsonObject != null)
            {
                return jsonObject.Name;
            }

            var typeName = type.Name;
            if (options.PluralizeEntities)
            {
                typeName = typeName.Pluralize();
            }

            return options.DocumentsCaseType.Convert(typeName);
        }

        public Type GetSequenceType()
        {
            Type? sequenceType = TryGetSequenceType(type);
            if (sequenceType == null)
            {
                throw new ArgumentException("Sequence type not found.");
            }

            return sequenceType;
        }

        public Type? TryGetSequenceType()
            => type.TryGetElementType(typeof(IEnumerable<>))
               ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));

        public Type? TryGetElementType(Type interfaceOrBaseType)
        {
            if (type.IsGenericTypeDefinition)
            {
                return null;
            }

            IEnumerable<Type>? types = GetGenericTypeImplementations(type, interfaceOrBaseType);

            Type? singleImplementation = null;
            foreach (Type? implementation in types)
            {
                if (singleImplementation == null)
                {
                    singleImplementation = implementation;
                }
                else
                {
                    singleImplementation = null;
                    break;
                }
            }

            return singleImplementation?.GenericTypeArguments.FirstOrDefault();
        }

        public IEnumerable<Type> GetGenericTypeImplementations(Type interfaceOrBaseType)
        {
            TypeInfo? typeInfo = type.GetTypeInfo();
            if (!typeInfo.IsGenericTypeDefinition)
            {
                IEnumerable<Type> baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                    ? typeInfo.ImplementedInterfaces
                    : type.GetBaseTypes();
                foreach (Type? baseType in baseTypes)
                {
                    if (baseType.IsGenericType
                        && baseType.GetGenericTypeDefinition() == interfaceOrBaseType)
                    {
                        yield return baseType;
                    }
                }

                if (type.IsGenericType
                    && type.GetGenericTypeDefinition() == interfaceOrBaseType)
                {
                    yield return type;
                }
            }
        }

        public bool IsEnumerable()
        {
            return type.IsArray || typeof(IEnumerable).IsAssignableFrom(type);
        }
    }

    private static IEnumerable<Type> GetBaseTypes(this Type? type)
    {
        type = type?.BaseType;

        while (type != null)
        {
            yield return type;

            type = type.BaseType;
        }
    }
}