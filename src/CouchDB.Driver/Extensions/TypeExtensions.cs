using System.Reflection;

namespace CouchDB.Driver.Extensions;

internal static class TypeExtensions
{
    extension(Type type)
    {
        public Type GetSequenceType()
        {
            Type? sequenceType = type.TryGetSequenceType();
            if (sequenceType == null)
            {
                throw new ArgumentException("Sequence type not found.");
            }

            return sequenceType;
        }

        private Type? TryGetSequenceType() =>
            type.TryGetElementType(typeof(IEnumerable<>)) ?? type.TryGetElementType(typeof(IAsyncEnumerable<>));

        private Type? TryGetElementType(Type interfaceOrBaseType)
        {
            if (type.IsGenericTypeDefinition)
            {
                return null;
            }

            IEnumerable<Type> types = type.GetGenericTypeImplementations(interfaceOrBaseType);

            Type? singleImplementation = null;
            foreach (Type implementation in types)
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

        private IEnumerable<Type> GetGenericTypeImplementations(Type interfaceOrBaseType)
        {
            TypeInfo typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericTypeDefinition)
            {
                yield break;
            }

            IEnumerable<Type> baseTypes = interfaceOrBaseType.GetTypeInfo().IsInterface
                ? typeInfo.ImplementedInterfaces
                : type.GetBaseTypes();
            foreach (Type baseType in baseTypes)
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