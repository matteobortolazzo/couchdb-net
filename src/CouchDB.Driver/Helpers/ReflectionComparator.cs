using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver.Helpers
{
    internal static class ReflectionComparator
    {
        public static bool IsCompatible(MethodInfo sourceMethodInfo, MethodInfo targetMethodInfo)
        {
            // If return types are not compatible
            if (!IsCompatible(sourceMethodInfo.ReturnType, targetMethodInfo.ReturnType))
            {
                return false;
            }

            ParameterInfo[] sourceParametersInfo = sourceMethodInfo.GetParameters();
            ParameterInfo[] targetParametersInfo = targetMethodInfo.GetParameters();
            
            // If different number of parameters
            if (sourceParametersInfo.Length != targetParametersInfo.Length)
            {
                return false;
            }

            // Checks if all parameters are compatibles
            for (var i = 0; i < sourceParametersInfo.Length; i++)
            {
                ParameterInfo sourceParameterInfo = sourceParametersInfo[i];
                ParameterInfo targetParameterInfo = targetParametersInfo[i];
                if (!IsCompatible(sourceParameterInfo, targetParameterInfo))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsCompatible(ParameterInfo sourceParameterInfo, ParameterInfo targetParameterInfo)
        {
            // If parameters types are LambdaExpression, take the generic argument value
            Type sourceParameterType = sourceParameterInfo.ParameterType;
            if (sourceParameterType.BaseType == typeof(LambdaExpression))
            {
                sourceParameterType = sourceParameterType.GenericTypeArguments.First();
            }
            Type targetParameterType = targetParameterInfo.ParameterType;
            if (targetParameterType.BaseType == typeof(LambdaExpression))
            {
                targetParameterType = targetParameterType.GenericTypeArguments.First();
            }

            // If parameters types are not compatible
            if (!IsCompatible(sourceParameterType, targetParameterType))
            {
                return false;
            }
            return true;
        }

        public static bool IsCompatible(Type sourceType, Type targetType)
        {
            // If the target type is a generic parameter, no other check needed
            if (targetType.IsGenericParameter)
            {
                return true;
            }

            // If the target type is generic
            if (!targetType.IsGenericType)
            {
                Type[] sourceGenericParameters = sourceType.GetGenericArguments();
                Type[] targetGenericParameters = targetType.GetGenericArguments();

                // If different number of generic parameters
                if (sourceGenericParameters.Length != targetGenericParameters.Length)
                {
                    return false;
                }

                // Checks if all generic parameters are compatibles
                for (var i = 0; i < sourceGenericParameters.Length; i++)
                {
                    Type sourceGenericParameter = sourceGenericParameters[i];
                    Type targetGenericParameter = targetGenericParameters[i];
                    if (!IsCompatible(sourceGenericParameter, targetGenericParameter))
                    {
                        return false;
                    }
                }

                // If all generic parameters are compatible
                return true;
            }

            // If the target is not generic, just check if types are different
            return sourceType == targetType;
        }
    }
}
