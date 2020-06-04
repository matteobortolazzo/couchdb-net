using System;
using System.Collections.Generic;

namespace CouchDB.Driver.Helpers
{
    internal static class TypeSystem
    {
        internal static Type GetElementType(Type seqType)
        {
            Type? enumerableInterface = FindIEnumerable(seqType);
            return enumerableInterface == null ? seqType : enumerableInterface.GetGenericArguments()[0];
        }

        private static Type? FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
            {
                return null;
            }

            if (seqType.IsArray)
            {
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());
            }

            if (seqType.IsGenericType)
            {
                foreach (Type arg in seqType.GetGenericArguments())
                {
                    Type enumerableInterface = typeof(IEnumerable<>).MakeGenericType(arg);

                    if (enumerableInterface.IsAssignableFrom(seqType))
                    {
                        return enumerableInterface;
                    }
                }
            }

            Type[] interfaces = seqType.GetInterfaces();

            if (interfaces != null && interfaces.Length > 0)
            {
                foreach (Type @interface in interfaces)
                {
                    Type? enumerableInterface = FindIEnumerable(@interface);
                    if (enumerableInterface != null)
                    {
                        return enumerableInterface;
                    }
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }

            return null;
        }
    }
}
