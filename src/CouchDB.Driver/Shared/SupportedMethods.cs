using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace CouchDB.Driver.Shared
{
    internal static class SupportedMethods
    {
        private static List<MethodInfo> Supported { get; }
        private static List<MethodInfo> Composite { get; }
        public static bool IsSupportedNativelyOrByComposition(this MethodInfo methodInfo)
            => methodInfo.IsSupportedByComposition() || Supported.Contains(methodInfo);

        public static bool IsSupportedByComposition(this MethodInfo methodInfo)
            => Composite.Contains(methodInfo)
               || QueryableMethods.IsSumWithSelector(methodInfo)
               || QueryableMethods.IsAverageWithSelector(methodInfo);

        static SupportedMethods()
        {
            Supported = new List<MethodInfo>
            {
                QueryableMethods.Where,
                QueryableMethods.OrderBy,
                QueryableMethods.ThenBy,
                QueryableMethods.OrderByDescending,
                QueryableMethods.ThenByDescending,
                QueryableMethods.Skip,
                QueryableMethods.Take,
                QueryableMethods.Select,

                SupportedQueryMethods.All,
                SupportedQueryMethods.AnyWithPredicate,
                SupportedQueryMethods.UseBookmark,
                SupportedQueryMethods.WithReadQuorum,
                SupportedQueryMethods.WithoutIndexUpdate,
                SupportedQueryMethods.FromStable,
                SupportedQueryMethods.UseIndex,
                SupportedQueryMethods.IncludeExecutionStats,
                SupportedQueryMethods.IncludeConflicts,
                SupportedQueryMethods.EnumerableContains,
                SupportedQueryMethods.FieldExists,
                SupportedQueryMethods.IsCouchType,
                SupportedQueryMethods.In,
                SupportedQueryMethods.IsMatch,
                SupportedQueryMethods.Contains
            };

            Composite = new List<MethodInfo>
            {
                QueryableMethods.MinWithoutSelector,
                QueryableMethods.MinWithSelector,
                QueryableMethods.MaxWithoutSelector,
                QueryableMethods.MaxWithSelector,
                QueryableMethods.AnyWithPredicate,
                QueryableMethods.All,
                QueryableMethods.FirstWithoutPredicate,
                QueryableMethods.FirstWithPredicate,
                QueryableMethods.FirstOrDefaultWithoutPredicate,
                QueryableMethods.FirstOrDefaultWithPredicate,
                QueryableMethods.SingleWithoutPredicate,
                QueryableMethods.SingleWithPredicate,
                QueryableMethods.SingleOrDefaultWithoutPredicate,
                QueryableMethods.SingleOrDefaultWithPredicate,
                QueryableMethods.LastWithoutPredicate,
                QueryableMethods.LastWithPredicate,
                QueryableMethods.LastOrDefaultWithoutPredicate,
                QueryableMethods.LastOrDefaultWithPredicate
            };
        }
    }
}