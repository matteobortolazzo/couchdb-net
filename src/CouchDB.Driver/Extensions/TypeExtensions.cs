using System;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using CouchDB.Driver.Attributes;

namespace CouchDB.Driver.Extensions;

internal static class TypeExtensions
{
    public static string GetDatabaseName<TSource>()
    {
        Type type = typeof(TSource);
        return type.GetDatabaseName();
    }

    extension(Type type)
    {
        public string GetDatabaseName()
        {
            ArgumentNullException.ThrowIfNull(type);
            DatabaseNameAttribute? attributes = type.GetCustomAttribute<DatabaseNameAttribute>(false);
            return attributes == null
                ? throw new ArgumentException("Type does not have DatabaseName attribute.", nameof(type))
                : attributes.Name;
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
            if (typeInfo.IsGenericTypeDefinition)
            {
                yield break;
            }

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