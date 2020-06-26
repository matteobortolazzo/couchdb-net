using System;
using System.Linq;
using System.Linq.Expressions;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.ExpressionVisitors
{
    public class QueryPreTranslator : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            Type[] genericArgs = node.Method.GetGenericArguments();

            // Return an expression representing Queryable<T>.Take(1)
            MethodCallExpression AddTakeOneExpression(Expression previousExpression, int numberOfElements = 1)
            {
                return Expression.Call(typeof(Queryable), nameof(Queryable.Take), genericArgs.Take(1).ToArray(), previousExpression, Expression.Constant(numberOfElements));
            }
            
            MethodCallExpression AddWhereExpression(Expression previousExpression, Expression predicate, bool negate = false)
            {
                if (negate)
                {
                    predicate = Expression.Not(predicate);
                }
                return Expression.Call(typeof(Queryable), nameof(Queryable.Where), genericArgs, previousExpression, predicate);
            }
            
            // Min(e => e.P) == OrderBy(e => e.P).Take(1) + Min
            if (node.IsMin())
            {
                MethodCallExpression orderByDesc = Expression.Call(typeof(Queryable), nameof(Queryable.OrderBy), genericArgs, node.Arguments[0], node.Arguments[1]);
                return AddTakeOneExpression(orderByDesc);
            }

            // Max(e => e.P) == OrderByDescending(e => e.P).Take(1) + Max
            if (node.IsMax())
            {
                MethodCallExpression orderBy = Expression.Call(typeof(Queryable), nameof(Queryable.OrderByDescending), genericArgs, node.Arguments[0], node.Arguments[1]);
                return AddTakeOneExpression(orderBy);
            }

            // Sum(e => e.P) == Select(e => new { e.P }) + Sum
            if (node.IsSum())
            {
                return Expression.Call(typeof(Queryable), nameof(Queryable.Select), genericArgs, node.Arguments[0], node.Arguments[1]);
            }

            // Average(e => e.P) == Select(e => new { e.P }) + Average
            if (node.IsAverage())
            {
                return Expression.Call(typeof(Queryable), nameof(Queryable.Average), genericArgs, node.Arguments[0], node.Arguments[1]);
            }

            // Any
            if (node.IsAny())
            {
                // Any() == Take(1) + Any
                if (node.HasParameterNumber(1))
                {
                    return AddTakeOneExpression(node.Arguments[0]);
                }
                // Any(e => e.P) == Where(e => e.P).Take(1) + Any
                if (node.HasParameterNumber(2))
                {
                    MethodCallExpression whereExpression = AddWhereExpression(node.Arguments[0], node.Arguments[1]);
                    return AddTakeOneExpression(whereExpression);
                }
            }

            // All
            if (node.IsAll())
            {
                // All() == Take(1) + All
                if (node.HasParameterNumber(1))
                {
                    return AddTakeOneExpression(node.Arguments[0]);
                }
                // All(e => e.P) == Where(e => !e.P).Take(1) + All
                if (node.HasParameterNumber(2))
                {
                    MethodCallExpression whereExpression = AddWhereExpression(node.Arguments[0], node.Arguments[1], true);
                    return AddTakeOneExpression(whereExpression);
                }
            }

            // Single
            if (node.IsSingle())
            {
                // Single() == Take(2) + Single
                if (node.HasParameterNumber(1))
                {
                    return AddTakeOneExpression(node.Arguments[0]);
                }
                // Single(e => e.P) == Where(e => e.P).Take(2) + Single
                if (node.HasParameterNumber(2))
                {
                    MethodCallExpression whereExpression = AddWhereExpression(node.Arguments[0], node.Arguments[1]);
                    return AddTakeOneExpression(whereExpression, 2);
                }
            }

            // First
            if (node.IsFirst())
            {
                // First() == Take(1) + First
                if (node.HasParameterNumber(1))
                {
                    return AddTakeOneExpression(node.Arguments[0]);
                }
                // First(e => e.P) == Where(e => e.P).Take(1) + First
                if (node.HasParameterNumber(2))
                {
                    MethodCallExpression whereExpression = AddWhereExpression(node.Arguments[0], node.Arguments[1]);
                    return AddTakeOneExpression(whereExpression);
                }
            }

            // Last
            if (node.IsLast())
            {
                // Last(e => e.P) == Where(e => e.P) + Last
                if (node.HasParameterNumber(2))
                {
                    MethodCallExpression whereExpression = AddWhereExpression(node.Arguments[0], node.Arguments[1]);
                    MethodCallExpression orderBy = Expression.Call(typeof(Queryable), nameof(Queryable.OrderByDescending), genericArgs, node.Arguments[0], node.Arguments[1]);
                    return AddTakeOneExpression(whereExpression);
                }
            }

            return base.VisitMethodCall(node);
        }
    }
}
