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

            // Min(d => d.Property) == OrderBy(d => d.Property).Take(1).Select(d => d.Property).Min()
            if (genericDefinition == QueryableMethods.MinWithSelector)
            {
                return node
                    .SubstituteWithQueryableCall(nameof(Queryable.OrderBy))
                    .WrapInTake(1)
                    .WrapInSelect(node)
                    .WrapInMethodWithoutSelector(QueryableMethods.MinWithoutSelector);
            }

            // Max(d => d.Property) == OrderByDescending(d => d.Property).Take(1).Select(d => d.Property).Max()
            if (genericDefinition == QueryableMethods.MaxWithSelector)
            {
                return node
                    .SubstituteWithQueryableCall(nameof(Queryable.OrderByDescending))
                    .WrapInTake(1)
                    .WrapInSelect(node)
                    .WrapInMethodWithoutSelector(QueryableMethods.MaxWithoutSelector);
            }

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

            // Any() => Take(1).Any()
            if (genericDefinition == QueryableMethods.AnyWithoutPredicate)
            {
                return node
                    .WrapInTake(1)
                    .WrapInMethodCall(node);
            }

            // Any(d => condition) == Where(d => condition).Take(1).Any(d => condition)
            if (genericDefinition == QueryableMethods.AnyWithPredicate)
            {
                return node
                    .SubstituteWithWhere()
                    .WrapInTake(1)
                    .WrapInMethodCall(node);
            }

            // All(d => condition) == Where(d => condition).Take(1).All(d => condition)
            if (genericDefinition == QueryableMethods.All)
            {
                return node
                    .SubstituteWithWhere(true)
                    .WrapInTake(1)
                    .WrapInMethodCall(node);
            }

            // Single() == Take(2).Single()
            if (genericDefinition == QueryableMethods.SingleWithoutPredicate ||
                genericDefinition == QueryableMethods.SingleOrDefaultWithoutPredicate)
            {
                return node
                    .SubstituteWithTake(2)
                    .WrapInMethodCall(node);
            }

            // Single(d => condition) == Where(d => condition).Take(2).Single(d => condition)
            if (genericDefinition == QueryableMethods.SingleWithPredicate ||
                genericDefinition == QueryableMethods.SingleOrDefaultWithPredicate)
            {
                return node
                    .SubstituteWithTake(2)
                    .WrapInMethodCall(node);
            }

            // First() == Take(1).First()
            if (genericDefinition == QueryableMethods.FirstWithoutPredicate ||
                genericDefinition == QueryableMethods.FirstOrDefaultWithoutPredicate)
            {
                return node
                    .SubstituteWithTake(1)
                    .WrapInMethodCall(node);
            }

            // First(d => condition) == Where(d => condition).Take(1).First(d => condition)
            if (genericDefinition == QueryableMethods.FirstWithPredicate ||
                genericDefinition == QueryableMethods.FirstOrDefaultWithPredicate)
            {
                return node
                    .SubstituteWithTake(1)
                    .WrapInMethodCall(node);
            }

            // Last() == Last()
            if (genericDefinition == QueryableMethods.LastWithoutPredicate ||
                genericDefinition == QueryableMethods.LastOrDefaultWithoutPredicate)
            {
                return node;
            }

            // Last(d => condition) == Where(d => condition).Last(d => condition)
            if (genericDefinition == QueryableMethods.LastWithPredicate ||
                genericDefinition == QueryableMethods.LastOrDefaultWithPredicate)
            {
                return node
                    .SubstituteWithWhere()
                    .WrapInMethodCall(node);
            }

            return base.VisitMethodCall(node);
        }
    }
}
