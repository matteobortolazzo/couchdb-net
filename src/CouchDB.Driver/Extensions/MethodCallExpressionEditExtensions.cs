using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CouchDB.Driver.Helpers;

namespace CouchDB.Driver.Extensions
{
    internal static class MethodCallExpressionEditExtensions
    {
        public static MethodCallExpression SubstituteWithQueryableCall(this MethodCallExpression node, string methodName)
        {
            Check.NotNull(node, nameof(node));

            return Expression.Call(typeof(Queryable), methodName, node.Method.GetGenericArguments(), node.Arguments[0], node.Arguments[1]);
        }

        public static MethodCallExpression SubstituteWithTake(this MethodCallExpression node, int numberOfElements)
        {
            Check.NotNull(node, nameof(node));

            return Expression.Call(typeof(Queryable), nameof(Queryable.Take),
                node.Method.GetGenericArguments().Take(1).ToArray(), node.Arguments[0], Expression.Constant(numberOfElements));
        }

        public static MethodCallExpression SubstituteWithSelect(this MethodCallExpression node, MethodCallExpression selectorNode)
        {
            Check.NotNull(node, nameof(node));

            Type selectorType = selectorNode.Arguments[1].GetSelectorType();
            Type[] genericArgumentTypes = node.Method
                .GetGenericArguments()
                .Append(selectorType)
                .ToArray();

            return Expression.Call(typeof(Queryable), nameof(Queryable.Select), genericArgumentTypes, node.Arguments[0], selectorNode.Arguments[1]);
        }

        public static MethodCallExpression SubstituteWithWhere(this MethodCallExpression node, bool negate = false)
        {
            Check.NotNull(node, nameof(node));

            Expression predicate = node.Arguments[1];

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

        public static MethodCallExpression WrapInTake(this MethodCallExpression node, int numberOfElements)
        {
            Check.NotNull(node, nameof(node));

            return Expression.Call(typeof(Queryable), nameof(Queryable.Take),
                node.Method.GetGenericArguments().Take(1).ToArray(), node, Expression.Constant(numberOfElements));
        }

        public static MethodCallExpression WrapInSelect(this MethodCallExpression node, MethodCallExpression selectorNode)
        {
            Check.NotNull(node, nameof(node));

            Type selectorType = selectorNode.Arguments[1].GetSelectorType();
            Type[] genericArgumentTypes = node.Method
                .GetGenericArguments()
                .Append(selectorType)
                .ToArray();

            return Expression.Call(typeof(Queryable), nameof(Queryable.Select), genericArgumentTypes, node, selectorNode.Arguments[1]);
        }

        public static MethodCallExpression WrapInAverageSum(this MethodCallExpression node, MethodCallExpression wrap)
        {
            Check.NotNull(node, nameof(node));
            Check.NotNull(wrap, nameof(wrap));

            Type selectorType = wrap.Arguments[1].GetSelectorType();
            Type queryableType = typeof(IQueryable<>).MakeGenericType(selectorType);
            MethodInfo numberMethod = typeof(Queryable).GetMethod(wrap.Method.Name, new []{queryableType});
            return Expression.Call(numberMethod, node);
        }

        public static MethodCallExpression WrapInMinMax(this MethodCallExpression node, MethodInfo methodInfo)
        {
            Check.NotNull(node, nameof(node));
            Check.NotNull(methodInfo, nameof(methodInfo));

            MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(node.Method.GetGenericArguments()[1]);
            return Expression.Call(genericMethodInfo, node);
        }

        public static MethodCallExpression WrapInMethodWithoutSelector(this MethodCallExpression node, MethodInfo methodInfo)
        {
            Check.NotNull(node, nameof(node));
            Check.NotNull(methodInfo, nameof(methodInfo));

            MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(node.Method.GetGenericArguments()[0]);
            return Expression.Call(genericMethodInfo, node);
        }

        private static Type GetSelectorType(this Expression selector)
        {
            if (selector is UnaryExpression u && u.Operand is LambdaExpression l && l.Body is MemberExpression m)
            {
                return m.Type;
            }

            throw new InvalidOperationException($"Expression of type {selector.GetType().Name} does not select a property.");
        }
    }
}