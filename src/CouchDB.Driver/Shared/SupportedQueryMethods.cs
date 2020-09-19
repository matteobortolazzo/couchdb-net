using System.Linq;
using System.Reflection;
using CouchDB.Driver.Query.Extensions;

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
        public static MethodInfo Select { get; }
        public static MethodInfo Convert { get; }
        public static MethodInfo EnumerableContains { get; }
        public static MethodInfo FieldExists { get; }
        public static MethodInfo IsCouchType { get; }
        public static MethodInfo In { get; }
        public static MethodInfo IsMatch { get; }
        public static MethodInfo Contains { get; }

        static SupportedQueryMethods()
        {
            var enumerableMethods = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
            All = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.All)
                      && mi.GetParameters().Length == 2);
            AnyWithPredicate = enumerableMethods.Single(
                mi => mi.Name == nameof(Enumerable.Any)
                      && mi.GetParameters().Length == 2);

            var queryableExtensionsMethods = typeof(QueryableQueryExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
            UseBookmark =
                queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableQueryExtensions.UseBookmark));
            WithReadQuorum =
                queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableQueryExtensions.WithReadQuorum));
            WithoutIndexUpdate =
                queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableQueryExtensions.WithoutIndexUpdate));
            FromStable =
                queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableQueryExtensions.FromStable));
            UseIndex = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableQueryExtensions.UseIndex));
            IncludeExecutionStats =
                queryableExtensionsMethods.Single(mi =>
                    mi.Name == nameof(QueryableQueryExtensions.IncludeExecutionStats));
            IncludeConflicts =
                queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableQueryExtensions.IncludeConflicts));
            Select = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableQueryExtensions.Select));
            Convert = queryableExtensionsMethods.Single(mi => mi.Name == nameof(QueryableQueryExtensions.Convert));

            EnumerableContains = typeof(EnumerableQueryExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(EnumerableQueryExtensions.Contains));

            var objectExtensionsMethods = typeof(ObjectQueryExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).ToList();
            FieldExists = objectExtensionsMethods.Single(mi => mi.Name == nameof(ObjectQueryExtensions.FieldExists));
            IsCouchType = objectExtensionsMethods.Single(mi => mi.Name == nameof(ObjectQueryExtensions.IsCouchType));
            In = objectExtensionsMethods.Single(mi => mi.Name == nameof(ObjectQueryExtensions.In));

            IsMatch = typeof(StringQueryExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(StringQueryExtensions.IsMatch));

            Contains = typeof(Enumerable)
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(mi => mi.Name == nameof(Enumerable.Contains) && mi.GetParameters().Length == 2);
        }
    }
}