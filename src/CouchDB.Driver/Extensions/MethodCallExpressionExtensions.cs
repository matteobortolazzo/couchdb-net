using System.Linq;
using System.Linq.Expressions;

namespace CouchDB.Driver.Extensions
{
    internal static class MethodCallExpressionExtensions
    {
        public static bool HasParameterNumber(this MethodCallExpression node, int number) =>
            node.Method.GetParameters().Length == number;

        public static bool HasName(this MethodCallExpression node, string name) =>
            node.Method.Name == name;
        
        public static bool IsAny(this MethodCallExpression node) =>
            node.HasName(nameof(Queryable.Any)) ||
            node.HasName(nameof(QueryableAsyncExtensions.AnyAsync));

        public static bool IsAll(this MethodCallExpression node) =>
            node.HasName(nameof(Queryable.All)) ||
            node.HasName(nameof(QueryableAsyncExtensions.AllAsync));

        public static bool IsSingle(this MethodCallExpression node) =>
            node.HasName(nameof(Queryable.Single)) ||
            node.HasName(nameof(Queryable.SingleOrDefault)) ||
            node.HasName(nameof(QueryableAsyncExtensions.SingleAsync)) ||
            node.HasName(nameof(QueryableAsyncExtensions.SingleOrDefaultAsync));

        public static bool IsFirst(this MethodCallExpression node) =>
            node.HasName(nameof(Queryable.First)) ||
            node.HasName(nameof(Queryable.FirstOrDefault)) ||
            node.HasName(nameof(QueryableAsyncExtensions.FirstAsync)) ||
            node.HasName(nameof(QueryableAsyncExtensions.FirstOrDefaultAsync));

        public static bool IsLast(this MethodCallExpression node) =>
            node.HasName(nameof(Queryable.Last)) ||
            node.HasName(nameof(Queryable.LastOrDefault)) ||
            node.HasName(nameof(QueryableAsyncExtensions.LastAsync)) ||
            node.HasName(nameof(QueryableAsyncExtensions.LastOrDefaultAsync));

        public static bool IsMin(this MethodCallExpression node) =>
            (node.HasParameterNumber(2) && node.HasName(nameof(Queryable.Min))) ||
            node.HasName(nameof(QueryableAsyncExtensions.MinAsync));

        public static bool IsMax(this MethodCallExpression node) =>
            (node.HasParameterNumber(2) && node.HasName(nameof(Queryable.Max))) ||
            node.HasName(nameof(QueryableAsyncExtensions.MaxAsync));

        public static bool IsSum(this MethodCallExpression node) =>
            (node.HasParameterNumber(2) && node.HasName(nameof(Queryable.Sum))) ||
            node.HasName(nameof(QueryableAsyncExtensions.SumAsync));

        public static bool IsAverage(this MethodCallExpression node) =>
            (node.HasParameterNumber(2) && node.HasName(nameof(Queryable.Average))) ||
            node.HasName(nameof(QueryableAsyncExtensions.AverageAsync));
    }
}
