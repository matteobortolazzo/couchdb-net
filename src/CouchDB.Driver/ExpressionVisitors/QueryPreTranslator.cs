using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;
using CouchDB.Driver.Shared;

namespace CouchDB.Driver.ExpressionVisitors
{
    public class QueryPreTranslator : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            MethodInfo genericDefinition = node.Method.GetGenericMethodDefinition();

            if (!genericDefinition.IsSupportedNativelyOrByComposition())
            {
                throw new NotSupportedException($"Method {node.Method.Name} cannot be converter to a valid query.");
            }

            #region Min/Max

            // Min(d => d.Property) == OrderBy(d => d.Property).Take(1).Select(d => d.Property).Min()
            if (genericDefinition == QueryableMethods.MinWithSelector)
            {
                return node
                    .SubstituteWithQueryableCall(nameof(Queryable.OrderBy))
                    .WrapInTake(1)
                    .WrapInSelect(node)
                    .WrapInMinMax(QueryableMethods.MinWithoutSelector);
            }

            // Max(d => d.Property) == OrderByDescending(d => d.Property).Take(1).Select(d => d.Property).Max()
            if (genericDefinition == QueryableMethods.MaxWithSelector)
            {
                return node
                    .SubstituteWithQueryableCall(nameof(Queryable.OrderByDescending))
                    .WrapInTake(1)
                    .WrapInSelect(node)
                    .WrapInMinMax(QueryableMethods.MaxWithoutSelector);
            }

            #endregion

            #region Sum/Average

            // Sum(d => d.Property) == Select(d => d.Property).Sum()
            if (QueryableMethods.IsSumWithSelector(genericDefinition))
            {
                return node
                    .SubstituteWithSelect(node)
                    .WrapInAverageSum(node);
            }

            // Average(d => d.Property) == Select(d => d.Property).Average()
            if (QueryableMethods.IsAverageWithSelector(genericDefinition))
            {
                return node
                    .SubstituteWithSelect(node)
                    .WrapInAverageSum(node);
            }

            #endregion

            #region Any/All

            // Any() => Take(1).Any()
            if (genericDefinition == QueryableMethods.AnyWithoutPredicate)
            {
                return node
                    .WrapInTake(1)
                    .WrapInMethodWithoutSelector(QueryableMethods.AnyWithoutPredicate);
            }

            // Any(d => condition) == Where(d => condition).Take(1).Any()
            if (genericDefinition == QueryableMethods.AnyWithPredicate)
            {
                return node
                    .SubstituteWithWhere()
                    .WrapInTake(1)
                    .WrapInMethodWithoutSelector(QueryableMethods.AnyWithoutPredicate);
            }

            // All(d => condition) == Where(d => !condition).Take(1).Any()
            if (genericDefinition == QueryableMethods.All)
            {
                return node
                    .SubstituteWithWhere(true)
                    .WrapInTake(1)
                    .WrapInMethodWithoutSelector(QueryableMethods.AnyWithoutPredicate);
            }

            #endregion

            #region Single/SingleOrDefault

            // Single() == Take(2).Single()
            if (genericDefinition == QueryableMethods.SingleWithoutPredicate)
            {
                return node
                    .SubstituteWithTake(2)
                    .WrapInMethodWithoutSelector(QueryableMethods.SingleWithoutPredicate);
            }

            // SingleOrDefault() == Take(2).SingleOrDefault()
            if (genericDefinition == QueryableMethods.SingleOrDefaultWithoutPredicate)
            {
                return node
                    .SubstituteWithTake(2)
                    .WrapInMethodWithoutSelector(QueryableMethods.SingleOrDefaultWithoutPredicate);
            }

            // Single(d => condition) == Where(d => condition).Take(2).Single()
            if (genericDefinition == QueryableMethods.SingleOrDefaultWithPredicate)
            {
                return node
                    .SubstituteWithWhere()
                    .SubstituteWithTake(2)
                    .WrapInMethodWithoutSelector(QueryableMethods.SingleWithoutPredicate);
            }

            // SingleOrDefault(d => condition) == Where(d => condition).Take(2).SingleOrDefault()
            if (genericDefinition == QueryableMethods.SingleOrDefaultWithPredicate)
            {
                return node
                    .SubstituteWithWhere()
                    .SubstituteWithTake(2)
                    .WrapInMethodWithoutSelector(QueryableMethods.SingleOrDefaultWithoutPredicate);
            }

            #endregion

            #region First/FirstOrDefault

            // First() == Take(1).First()
            if (genericDefinition == QueryableMethods.FirstWithoutPredicate)
            {
                return node
                    .SubstituteWithTake(1)
                    .WrapInMethodWithoutSelector(QueryableMethods.FirstWithoutPredicate);
            }

            // FirstOrDefault() == Take(1).FirstOrDefault()
            if (genericDefinition == QueryableMethods.FirstOrDefaultWithoutPredicate)
            {
                return node
                    .SubstituteWithTake(1)
                    .WrapInMethodWithoutSelector(QueryableMethods.FirstOrDefaultWithoutPredicate);
            }

            // First(d => condition) == Where(d => condition).Take(1).First()
            if (genericDefinition == QueryableMethods.FirstWithPredicate)
            {
                return node
                    .SubstituteWithWhere()
                    .SubstituteWithTake(1)
                    .WrapInMethodWithoutSelector(QueryableMethods.FirstWithoutPredicate);
            }

            // FirstOrDefault(d => condition) == Where(d => condition).Take(1).FirstOrDefault()
            if (genericDefinition == QueryableMethods.FirstOrDefaultWithPredicate)
            {
                return node
                    .SubstituteWithWhere()
                    .SubstituteWithTake(1)
                    .WrapInMethodWithoutSelector(QueryableMethods.FirstOrDefaultWithoutPredicate);
            }

            #endregion

            #region Last/LastOrDefault

            // Last() == Last()
            // LastOrDefault() == LastOrDefault()
            if (genericDefinition == QueryableMethods.LastWithoutPredicate ||
                genericDefinition == QueryableMethods.LastOrDefaultWithoutPredicate)
            {
                return node;
            }

            // Last(d => condition) == Where(d => condition).Last()
            if (genericDefinition == QueryableMethods.LastWithPredicate)
            {
                return node
                    .SubstituteWithWhere()
                    .WrapInMethodWithoutSelector(QueryableMethods.LastWithoutPredicate);
            }

            // LastOrDefault(d => condition) == Where(d => condition).LastOrDefault()
            if (genericDefinition == QueryableMethods.LastOrDefaultWithPredicate)
            {
                return node
                    .SubstituteWithWhere()
                    .WrapInMethodWithoutSelector(QueryableMethods.LastOrDefaultWithoutPredicate);
            }

            #endregion

            return base.VisitMethodCall(node);
        }
    }
}
