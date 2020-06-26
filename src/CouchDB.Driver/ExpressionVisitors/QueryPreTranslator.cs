using System;
using System.Linq;
using System.Linq.Expressions;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.ExpressionVisitors
{
    public class QueryPreTranslator : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            Type[] genericArgs = node.Method.GetGenericArguments();
            var numberOfParameters = node.Method.GetParameters().Length;

            // Return an expression representing Queryable<T>.Take(1)
            MethodCallExpression GetTakeOneExpression(Expression previousExpression, int numberOfElements = 1)
            {
                return Expression.Call(typeof(Queryable), nameof(Queryable.Take), genericArgs.Take(1).ToArray(), previousExpression, Expression.Constant(numberOfElements));
            }

            // Min(e => e.P) == OrderBy(e => e.P).Take(1) + Min
            if (node.Method.Name == nameof(Queryable.Min) && numberOfParameters == 2)
            {
                MethodCallExpression orderByDesc = Expression.Call(typeof(Queryable), nameof(Queryable.OrderBy), genericArgs, node.Arguments[0], node.Arguments[1]);
                return GetTakeOneExpression(orderByDesc);
            }
            // Max(e => e.P) == OrderByDescending(e => e.P).Take(1) + Max
            if (node.Method.Name == nameof(Queryable.Max) && numberOfParameters == 2)
            {
                MethodCallExpression orderBy = Expression.Call(typeof(Queryable), nameof(Queryable.OrderByDescending), genericArgs, node.Arguments[0], node.Arguments[1]);
                return GetTakeOneExpression(orderBy);
            }
            // First and FirstOrDefault have the same behaviour
            if (node.Method.Name == nameof(Queryable.First) || node.Method.Name == nameof(Queryable.FirstOrDefault))
            {
                // First() == Take(1) + First
                if (numberOfParameters == 1)
                {
                    return GetTakeOneExpression(node.Arguments[0]);
                }
                // First(e => e.P) == Where(e => e.P).Take(1) + First
                if (numberOfParameters == 2)
                {
                    MethodCallExpression whereExpression = Expression.Call(typeof(Queryable), nameof(Queryable.Where), genericArgs, node.Arguments[0], node.Arguments[1]);
                    return GetTakeOneExpression(whereExpression);
                }
            }
            // Single and SingleOrDefault have the same behaviour
            if (node.Method.Name == nameof(Queryable.Single) || node.Method.Name == nameof(Queryable.SingleOrDefault))
            {
                // Single() == Take(2) + Single
                if (numberOfParameters == 1)
                {
                    return GetTakeOneExpression(node.Arguments[0], 2);
                }
                // SingleOrDefault(e => e.P) == Where(e => e.P).Take(2) + Single
                if (numberOfParameters == 2)
                {
                    MethodCallExpression whereExpression = Expression.Call(typeof(Queryable), nameof(Queryable.Where), genericArgs, node.Arguments[0], node.Arguments[1]);
                    return GetTakeOneExpression(whereExpression, 2);
                }
            }
            return base.VisitMethodCall(node);
        }
    }
}
