using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver.Helpers
{
    public static class MethodsFinder
    {
        public static MethodInfo ToEnumerable(this MethodInfo qminfo)
        {
            bool FilterMethods(MethodInfo emInfo)
            {
                if (qminfo.Name != emInfo.Name)
                {
                    return false;
                }

                Debug.WriteLine($"INIZIO {emInfo.Name}, {emInfo.ReturnType.Name}, {emInfo.GetParameters().Select(p => p.ParameterType.Name)}");
                return IsCompatible(qminfo, emInfo);
            }

            var methods = typeof(Enumerable).GetMethods().Where(FilterMethods).ToList();
            return methods.Single();
        }

        private static bool IsCompatible(ParameterInfo fromInfo, ParameterInfo toInfo)
        {
            Type qmParamMemberInfo = fromInfo.ParameterType;

            if (qmParamMemberInfo.BaseType == typeof(LambdaExpression))
            {
                qmParamMemberInfo = qmParamMemberInfo.GenericTypeArguments[0];
            }

            Type emParamMemberInfo = toInfo.ParameterType;
            if (!IsCompatible(qmParamMemberInfo, emParamMemberInfo))
            {
                return false;
            }
            return true;
        }

        private static bool IsCompatible(MethodInfo fromInfo, MethodInfo toInfo)
        {
            if (!IsCompatible(fromInfo.ReturnType, toInfo.ReturnType))
            {
                Debug.WriteLine($"MORTO MethodInfo ReturnType");
                return false;
            }

            ParameterInfo[] fromArgs = fromInfo.GetParameters();
            ParameterInfo[] toArgs = toInfo.GetParameters();

            if (fromArgs.Length != toArgs.Length)
            {
                Debug.WriteLine($"MORTO MethodInfo Param length");
                return false;
            }

            for (var i = 0; i < fromArgs.Length; i++)
            {
                ParameterInfo qmP = fromArgs[i];
                ParameterInfo emP = toArgs[i];
                if (!IsCompatible(qmP, emP))
                {
                    Debug.WriteLine($"MORTO MethodInfo Param #{i}");
                    return false;
                }
            }

            return true;
        }

        private static bool IsCompatible(Type fromType, Type toType)
        {
            if (fromType == toType)
            {
                return true;
            }

            var fromIsAction = IsActionOrFunc(fromType);
            var toIsAction = IsActionOrFunc(toType);
            if (fromIsAction != toIsAction)
            {
                return false;
            }

            if (fromIsAction && toIsAction)
            {
                Type[] genFromArguments = fromType.GetGenericArguments();
                Type[] genToArguments = toType.GetGenericArguments();
                if (genFromArguments.Length != genToArguments.Length)
                {
                    Debug.WriteLine($"MORTO Type action Param length");
                    return false;
                }

                for (var i = 0; i < genFromArguments.Length; i++)
                {
                    Type qmT = genFromArguments[i];
                    Type emT = genToArguments[i];
                    if (!IsCompatible(qmT, emT))
                    {
                        Debug.WriteLine($"MORTO Type action Param #{i}");
                        return false;
                    }
                }
                return true;
            }

            if (toType.IsGenericParameter)
            {
                return true;
            }

            if (toType.IsGenericType)
            {
                Type[] fromGenPararms = fromType.GetGenericArguments();
                Type[] toGenPararms = toType.GetGenericArguments();

                if (fromGenPararms.Length != toGenPararms.Length)
                {
                    Debug.WriteLine($"MORTO Type Param length");
                    return false;
                }

                for (var i = 0; i < fromGenPararms.Length; i++)
                {
                    Type qmT = fromGenPararms[i];
                    Type emT = toGenPararms[i];
                    if (!IsCompatible(qmT, emT))
                    {
                        Debug.WriteLine($"MORTO Type Param #{i}");
                        return false;
                    }
                }
                return true;
            }

            if (fromType != toType)
            {
                Debug.WriteLine($"MORTO Type different");
                return false;
            }
            return true;
        }

        static bool IsActionOrFunc(Type type)
        {
            if (type == typeof(Action)) return true;
            Type generic = null;
            if (type.IsGenericTypeDefinition) generic = type;
            else if (type.IsGenericType) generic = type.GetGenericTypeDefinition();
            if (generic == null) return false;
            if (generic == typeof(Action<>)) return true;
            if (generic == typeof(Action<,>)) return true;
            if (generic == typeof(Action<,,>)) return true;
            if (generic == typeof(Action<,,,>)) return true;
            if (generic == typeof(Func<>)) return true;
            if (generic == typeof(Func<,>)) return true;
            if (generic == typeof(Func<,,>)) return true;
            if (generic == typeof(Func<,,,>)) return true;
            return false;
        }
    }
}
