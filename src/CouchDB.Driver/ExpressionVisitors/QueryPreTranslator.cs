using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.ExpressionVisitors
{
    internal static class MethodCallExpressionEditExtensions
    {
        public static MethodCallExpression WrapInQueryableCall(this MethodCallExpression node, string methodName)
        {
            Check.NotNull(node, nameof(node));

            return Expression.Call(typeof(Queryable), methodName, node.Method.GetGenericArguments(), node.Arguments[0], node.Arguments[1]);
        }

        public static MethodCallExpression WrapInTake(this MethodCallExpression node, int numberOfElements)
        {
            Check.NotNull(node, nameof(node));

            return Expression.Call(typeof(Queryable), nameof(Queryable.Take),
                node.Method.GetGenericArguments().Take(1).ToArray(), node.Arguments[0], Expression.Constant(numberOfElements));
        }
        
        public static MethodCallExpression WrapInWhere(this MethodCallExpression node, bool negate = false)
        {
            Check.NotNull(node, nameof(node));

            var predicate = node.Arguments[1];

            if (negate)
            {
                if (predicate == null || !(predicate is UnaryExpression unary) || !(unary.Operand is LambdaExpression lambdaExpression))
                {
                    throw new InvalidOperationException();
                }

                UnaryExpression body = Expression.Not(lambdaExpression.Body);
                lambdaExpression = Expression.Lambda(body, lambdaExpression.Parameters);
                predicate = Expression.Quote(lambdaExpression);
            }

            return Expression.Call(typeof(Queryable), nameof(Queryable.Where), node.Method.GetGenericArguments(), node.Arguments[0], predicate);
        }

        public static MethodCallExpression WrapInMethodCall(this MethodCallExpression node, MethodCallExpression wrap)
        {
            Check.NotNull(node, nameof(node));
            Check.NotNull(wrap, nameof(wrap));
            
            var arguments = new List<Expression> {node};
            arguments.AddRange(wrap.Arguments.Skip(1));
            
            return Expression.Call(wrap.Method.DeclaringType, wrap.Method.Name, wrap.Method.GetGenericArguments(), arguments.ToArray());
        }
    }

    public class QueryPreTranslator : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            if (!QueryTranslator.CompositeQueryableMethods.Contains(node.Method.Name) &&
                !QueryTranslator.NativeQueryableMethods.Contains(node.Method.Name))
            {
                throw new NotSupportedException();
            }

            // Min(e => e.P) == OrderBy(e => e.P).Take(1) + Min
            if (node.IsMin())
            {
                return node
                    .WrapInQueryableCall(nameof(Queryable.OrderBy))
                    .WrapInTake(1)
                    .WrapInMethodCall(node);
            }

            // Max(e => e.P) == OrderByDescending(e => e.P).Take(1) + Max
            if (node.IsMax())
            {
                return node
                    .WrapInQueryableCall(nameof(Queryable.OrderByDescending))
                    .WrapInTake(1)
                    .WrapInMethodCall(node);
            }

            //// Sum(e => e.P) == Select(e => new { e.P }) + Sum
            //if (node.IsSum())
            //{
            //    return Expression.Call(typeof(Queryable), nameof(Queryable.Select), genericArgs, node.Arguments[0], node.Arguments[1]);
            //}

            //// Average(e => e.P) == Select(e => new { e.P }) + Average
            //if (node.IsAverage())
            //{
            //    return Expression.Call(typeof(Queryable), nameof(Queryable.Average), genericArgs, node.Arguments[0], node.Arguments[1]);
            //}

            // Any
            if (node.IsAny())
            {
                // Any(e => e.P) == Where(e => e.P).Take(1) + Any
                if (node.HasParameterNumber(2))
                {
                    node = node.WrapInWhere();
                }

                return node
                    .WrapInTake(1)
                    .WrapInMethodCall(node);
            }

            if (node.IsAll())
            {
                // All(e => e.P) == Where(e => !e.P).Take(1) + All
                if (node.HasParameterNumber(2))
                {
                    node = node.WrapInWhere(true);
                }

                return node
                    .WrapInTake(1)
                    .WrapInMethodCall(node);
            }

            // Single
            if (node.IsSingle())
            {
                // Single() == Take(2) + Single
                if (node.HasParameterNumber(2))
                {
                    node = node.WrapInWhere();
                }

                // Single(e => e.P) == Where(e => e.P).Take(2) + Single
                return node
                    .WrapInTake(2)
                    .WrapInMethodCall(node);
            }

            // First
            if (node.IsFirst())
            {
                // First() == Take(2) + First
                if (node.HasParameterNumber(2))
                {
                    node = node.WrapInWhere();
                }

                // First(e => e.P) == Where(e => e.P).Take(2) + First
                return node
                    .WrapInTake(1)
                    .WrapInMethodCall(node);
            }

            // Last
            if (node.IsLast())
            {
                // Last(e => e.P) == Where(e => e.P) + Last
                if (node.HasParameterNumber(2))
                {
                    return node
                        .WrapInWhere()
                        .WrapInMethodCall(node);
                }

                return node;
            }

            return base.VisitMethodCall(node);
        }
    }
}
