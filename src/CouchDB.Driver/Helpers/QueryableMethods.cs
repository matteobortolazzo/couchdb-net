using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver.Helpers
{
    internal static class QueryableMethods
    {
        public static MethodInfo All { get; }
        public static MethodInfo AnyWithoutPredicate { get; }
        public static MethodInfo AnyWithPredicate { get; }

        public static MethodInfo MinWithSelector { get; }
        public static MethodInfo MinWithoutSelector { get; }
        public static MethodInfo MaxWithSelector { get; }
        public static MethodInfo MaxWithoutSelector { get; }

        public static MethodInfo FirstWithoutPredicate { get; }
        public static MethodInfo FirstWithPredicate { get; }
        public static MethodInfo FirstOrDefaultWithoutPredicate { get; }
        public static MethodInfo FirstOrDefaultWithPredicate { get; }
        public static MethodInfo SingleWithoutPredicate { get; }
        public static MethodInfo SingleWithPredicate { get; }
        public static MethodInfo SingleOrDefaultWithoutPredicate { get; }
        public static MethodInfo SingleOrDefaultWithPredicate { get; }
        public static MethodInfo LastWithoutPredicate { get; }
        public static MethodInfo LastWithPredicate { get; }
        public static MethodInfo LastOrDefaultWithoutPredicate { get; }
        public static MethodInfo LastOrDefaultWithPredicate { get; }
        
        public static bool IsSumWithSelector(MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            return methodInfo.IsGenericMethod
                   && SumWithSelectorMethods.Values.Contains(methodInfo.GetGenericMethodDefinition());
        }

        public static bool IsAverageWithSelector(MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            return methodInfo.IsGenericMethod
                   && AverageWithSelectorMethods.Values.Contains(methodInfo.GetGenericMethodDefinition());
        }

        public static MethodInfo GetSumWithSelector(Type type)
        {
            Check.NotNull(type, nameof(type));

            return SumWithSelectorMethods[type];
        }
        
        public static MethodInfo GetAverageWithSelector(Type type)
        {
            Check.NotNull(type, nameof(type));

            return AverageWithSelectorMethods[type];
        }

        private static Dictionary<Type, MethodInfo> SumWithSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> AverageWithSelectorMethods { get; }

        static QueryableMethods()
        {
            var queryableMethods = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();

            All = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.All)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            AnyWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Any) && mi.GetParameters().Length == 1);
            AnyWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Any)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));

            MinWithSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Min)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            MinWithoutSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Min) && mi.GetParameters().Length == 1);
            MaxWithSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Max)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            MaxWithoutSelector = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Max) && mi.GetParameters().Length == 1);

            FirstWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.First) && mi.GetParameters().Length == 1);
            FirstWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.First)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            FirstOrDefaultWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.FirstOrDefault) && mi.GetParameters().Length == 1);
            FirstOrDefaultWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.FirstOrDefault)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SingleWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Single) && mi.GetParameters().Length == 1);
            SingleWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Single)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            SingleOrDefaultWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SingleOrDefault) && mi.GetParameters().Length == 1);
            SingleOrDefaultWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.SingleOrDefault)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            LastWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Last) && mi.GetParameters().Length == 1);
            LastWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Last)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            LastOrDefaultWithoutPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LastOrDefault) && mi.GetParameters().Length == 1);
            LastOrDefaultWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.LastOrDefault)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));

            SumWithSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithSelector<decimal>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(long), GetSumOrAverageWithSelector<long>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(int), GetSumOrAverageWithSelector<int>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(double), GetSumOrAverageWithSelector<double>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(float), GetSumOrAverageWithSelector<float>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(decimal?), GetSumOrAverageWithSelector<decimal?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(long?), GetSumOrAverageWithSelector<long?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(int?), GetSumOrAverageWithSelector<int?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(double?), GetSumOrAverageWithSelector<double?>(queryableMethods, nameof(Queryable.Sum)) },
                { typeof(float?), GetSumOrAverageWithSelector<float?>(queryableMethods, nameof(Queryable.Sum)) }
            };

            AverageWithSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                { typeof(decimal), GetSumOrAverageWithSelector<decimal>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(long), GetSumOrAverageWithSelector<long>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(int), GetSumOrAverageWithSelector<int>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(double), GetSumOrAverageWithSelector<double>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(float), GetSumOrAverageWithSelector<float>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(decimal?), GetSumOrAverageWithSelector<decimal?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(long?), GetSumOrAverageWithSelector<long?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(int?), GetSumOrAverageWithSelector<int?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(double?), GetSumOrAverageWithSelector<double?>(queryableMethods, nameof(Queryable.Average)) },
                { typeof(float?), GetSumOrAverageWithSelector<float?>(queryableMethods, nameof(Queryable.Average)) }
            };

            static MethodInfo GetSumOrAverageWithSelector<T>(IEnumerable<MethodInfo> queryableMethods, string methodName)
                => queryableMethods.Single(
                    mi => mi.Name == methodName
                          && mi.GetParameters().Length == 2
                          && IsSelector<T>(mi.GetParameters()[1].ParameterType));

            static bool IsExpressionOfFunc(Type type, int funcGenericArgs = 2)
                => type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(Expression<>)
                   && type.GetGenericArguments()[0].IsGenericType
                   && type.GetGenericArguments()[0].GetGenericArguments().Length == funcGenericArgs;

            static bool IsSelector<T>(Type type)
                => type.IsGenericType
                   && type.GetGenericTypeDefinition() == typeof(Expression<>)
                   && type.GetGenericArguments()[0].IsGenericType
                   && type.GetGenericArguments()[0].GetGenericArguments().Length == 2
                   && type.GetGenericArguments()[0].GetGenericArguments()[1] == typeof(T);
        }
    }
}