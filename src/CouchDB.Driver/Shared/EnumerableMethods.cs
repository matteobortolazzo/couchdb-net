using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.Shared
{
    internal static class EnumerableMethods
    {
        public static MethodInfo GetEnumerableEquivalent(MethodInfo methodInfo)
        {
            if (QueryableToEnumerableMethods.ContainsKey(methodInfo))
            {
                return QueryableToEnumerableMethods[methodInfo];
            }

            if (QueryableToEnumerableMethods.ContainsKey(methodInfo.GetGenericMethodDefinition()))
            {
                return QueryableToEnumerableMethods[methodInfo.GetGenericMethodDefinition()];
            }

            if (IsMinWithoutSelector(methodInfo))
            {
                return GetMinWithoutSelector(methodInfo.ReturnType);
            }

            if (IsMaxWithoutSelector(methodInfo))
            {
                return GetMaxWithoutSelector(methodInfo.ReturnType);
            }

            throw new InvalidOperationException($"Method {methodInfo.Name} not supported.");
        }

        public static bool IsMinWithoutSelector(MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            return MinWithoutSelectorMethods.ContainsKey(methodInfo.ReturnType);
        }

        public static bool IsMaxWithoutSelector(MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            return MinWithoutSelectorMethods.ContainsKey(methodInfo.ReturnType);
        }

        private static MethodInfo GetMinWithoutSelector(Type type)
        {
            Check.NotNull(type, nameof(type));

            return MinWithoutSelectorMethods[type];
        }

        private static MethodInfo GetMaxWithoutSelector(Type type)
        {
            Check.NotNull(type, nameof(type));

            return MaxWithoutSelectorMethods[type];
        }
        
        private static Dictionary<Type, MethodInfo> MinWithoutSelectorMethods { get; }
        private static Dictionary<Type, MethodInfo> MaxWithoutSelectorMethods { get; }
        private static Dictionary<MethodInfo, MethodInfo> QueryableToEnumerableMethods { get; }

        static EnumerableMethods()
        {
            #region Enumerable

            var enumerableMethods = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
         
            MethodInfo anyWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Any) && mi.GetParameters().Length == 1);

            MethodInfo firstWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.First) && mi.GetParameters().Length == 1);
            MethodInfo firstOrDefaultWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.FirstOrDefault) && mi.GetParameters().Length == 1);

            MethodInfo singleWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Single) && mi.GetParameters().Length == 1);
            MethodInfo singleOrDefaultWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.SingleOrDefault) && mi.GetParameters().Length == 1);
            MethodInfo lastWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Last) && mi.GetParameters().Length == 1);
            MethodInfo lastOrDefaultWithoutPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.LastOrDefault) && mi.GetParameters().Length == 1);

            MethodInfo select = enumerableMethods.First(mi =>
                mi.Name == nameof(Queryable.Select) && mi.GetParameters().Length == 2);


            QueryableToEnumerableMethods = new Dictionary<MethodInfo, MethodInfo>
            {
                {QueryableMethods.AnyWithoutPredicate, anyWithoutPredicate},
                {QueryableMethods.FirstWithoutPredicate, firstWithoutPredicate},
                {QueryableMethods.FirstOrDefaultWithoutPredicate, firstOrDefaultWithoutPredicate},
                {QueryableMethods.SingleWithoutPredicate, singleWithoutPredicate},
                {QueryableMethods.SingleOrDefaultWithoutPredicate, singleOrDefaultWithoutPredicate},
                {QueryableMethods.LastWithoutPredicate, lastWithoutPredicate},
                {QueryableMethods.LastOrDefaultWithoutPredicate, lastOrDefaultWithoutPredicate},
                {QueryableMethods.Select, select}
            };

            #endregion

            #region Sum/Average

            void AddSumOrAverage<T>(string methodName)
            {
                MethodInfo queryableMethod = methodName == nameof(Enumerable.Sum)
                    ? QueryableMethods.GetSumWithoutSelector(typeof(T))
                    : QueryableMethods.GetAverageWithoutSelector(typeof(T));
                MethodInfo enumerableMethod = enumerableMethods.GetSumOrAverageWithoutSelector<T>(methodName);
                QueryableToEnumerableMethods.Add(queryableMethod, enumerableMethod);
            }

            AddSumOrAverage<decimal>(nameof(Enumerable.Sum));
            AddSumOrAverage<long>(nameof(Enumerable.Sum));
            AddSumOrAverage<int>(nameof(Enumerable.Sum));
            AddSumOrAverage<double>(nameof(Enumerable.Sum));
            AddSumOrAverage<float>(nameof(Enumerable.Sum));
            AddSumOrAverage<decimal?>(nameof(Enumerable.Sum));
            AddSumOrAverage<long?>(nameof(Enumerable.Sum));
            AddSumOrAverage<int?>(nameof(Enumerable.Sum));
            AddSumOrAverage<double?>(nameof(Enumerable.Sum));
            AddSumOrAverage<float?>(nameof(Enumerable.Sum));

            AddSumOrAverage<decimal>(nameof(Enumerable.Average));
            AddSumOrAverage<long>(nameof(Enumerable.Average));
            AddSumOrAverage<int>(nameof(Enumerable.Average));
            AddSumOrAverage<double>(nameof(Enumerable.Average));
            AddSumOrAverage<float>(nameof(Enumerable.Average));
            AddSumOrAverage<decimal?>(nameof(Enumerable.Average));
            AddSumOrAverage<long?>(nameof(Enumerable.Average));
            AddSumOrAverage<int?>(nameof(Enumerable.Average));
            AddSumOrAverage<double?>(nameof(Enumerable.Average));
            AddSumOrAverage<float?>(nameof(Enumerable.Average));

            #endregion

            #region Min/Max

            MinWithoutSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                {typeof(decimal), enumerableMethods.GetMinOrMaxWithoutSelector<decimal>(nameof(Enumerable.Min))},
                {typeof(long), enumerableMethods.GetMinOrMaxWithoutSelector<long>(nameof(Enumerable.Min))},
                {typeof(int), enumerableMethods.GetMinOrMaxWithoutSelector<int>(nameof(Enumerable.Min))},
                {typeof(double), enumerableMethods.GetMinOrMaxWithoutSelector<double>(nameof(Enumerable.Min))},
                {typeof(float), enumerableMethods.GetMinOrMaxWithoutSelector<float>(nameof(Enumerable.Min))},
                {typeof(decimal?), enumerableMethods.GetMinOrMaxWithoutSelector<decimal?>(nameof(Enumerable.Min))},
                {typeof(long?), enumerableMethods.GetMinOrMaxWithoutSelector<long?>(nameof(Enumerable.Min))},
                {typeof(int?), enumerableMethods.GetMinOrMaxWithoutSelector<int?>(nameof(Enumerable.Min))},
                {typeof(double?), enumerableMethods.GetMinOrMaxWithoutSelector<double?>(nameof(Enumerable.Min))},
                {typeof(float?), enumerableMethods.GetMinOrMaxWithoutSelector<float?>(nameof(Enumerable.Min))}
            };

            MaxWithoutSelectorMethods = new Dictionary<Type, MethodInfo>
            {
                {typeof(decimal), enumerableMethods.GetMinOrMaxWithoutSelector<decimal>(nameof(Enumerable.Max))},
                {typeof(long), enumerableMethods.GetMinOrMaxWithoutSelector<long>(nameof(Enumerable.Max))},
                {typeof(int), enumerableMethods.GetMinOrMaxWithoutSelector<int>(nameof(Enumerable.Max))},
                {typeof(double), enumerableMethods.GetMinOrMaxWithoutSelector<double>(nameof(Enumerable.Max))},
                {typeof(float), enumerableMethods.GetMinOrMaxWithoutSelector<float>(nameof(Enumerable.Max))},
                {typeof(decimal?), enumerableMethods.GetMinOrMaxWithoutSelector<decimal?>(nameof(Enumerable.Max))},
                {typeof(long?), enumerableMethods.GetMinOrMaxWithoutSelector<long?>(nameof(Enumerable.Max))},
                {typeof(int?), enumerableMethods.GetMinOrMaxWithoutSelector<int?>(nameof(Enumerable.Max))},
                {typeof(double?), enumerableMethods.GetMinOrMaxWithoutSelector<double?>(nameof(Enumerable.Max))},
                {typeof(float?), enumerableMethods.GetMinOrMaxWithoutSelector<float?>(nameof(Enumerable.Max))}
            };

            #endregion
        }
    }
}