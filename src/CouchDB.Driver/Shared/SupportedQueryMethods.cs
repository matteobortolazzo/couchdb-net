using System;
using System.Linq;
using System.Reflection;
using CouchDB.Driver.Extensions;

namespace CouchDB.Driver.Shared
{
    internal static class SupportedQueryMethods
    {
        public static MethodInfo All { get; }
        public static MethodInfo AnyWithPredicate { get; }
        public static MethodInfo UseBookmark { get; }
        public static MethodInfo WithReadQuorum { get; }
        public static MethodInfo WithoutIndexUpdate { get; }
        public static MethodInfo FromStable { get; }
        public static MethodInfo UseIndex { get; }
        public static MethodInfo IncludeExecutionStats { get; }
        public static MethodInfo IncludeConflicts { get; }
        public static MethodInfo EnumerableContains { get; }
        public static MethodInfo FieldExists { get; }
        public static MethodInfo IsCouchType { get; }
        public static MethodInfo In { get; }
        public static MethodInfo IsMatch { get; }
        public static MethodInfo Contains { get; }

        static SupportedQueryMethods()
        {
            var queryableMethods = typeof(Queryable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
            All = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.All)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));
            AnyWithPredicate = queryableMethods.Single(
                mi => mi.Name == nameof(Queryable.Any)
                      && mi.GetParameters().Length == 2
                      && IsExpressionOfFunc(mi.GetParameters()[1].ParameterType));

            var queryableExtensionsMethods = typeof(QueryableExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
            UseBookmark = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableExtensions.UseBookmark));
            WithReadQuorum = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableExtensions.WithReadQuorum));
            WithoutIndexUpdate = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableExtensions.WithoutIndexUpdate));
            FromStable = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableExtensions.FromStable));
            UseIndex = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableExtensions.UseIndex));
            IncludeExecutionStats = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableExtensions.IncludeExecutionStats));
            IncludeConflicts = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableExtensions.IncludeConflicts));
            
            EnumerableContains = typeof(EnumerableExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(EnumerableExtensions.Contains));

            var objectExtensionsMethods = typeof(ObjectExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
            FieldExists = objectExtensionsMethods.Single(mi => mi.Name == nameof(ObjectExtensions.FieldExists));
            IsCouchType = objectExtensionsMethods.Single(mi => mi.Name == nameof(ObjectExtensions.IsCouchType));
            In = objectExtensionsMethods.Single(mi => mi.Name == nameof(ObjectExtensions.In));

            IsMatch = typeof(StringExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(StringExtensions.IsMatch));

            Contains = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(Enumerable.Contains) && mi.GetParameters().Length == 2);
        }

        static bool IsExpressionOfFunc(Type type, int funcGenericArgs = 2) =>
            type.IsExpressionOfFunc(funcGenericArgs);
    }
}