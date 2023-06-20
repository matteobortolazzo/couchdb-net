using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Helpers
{
    internal static class MethodCallExpressionBuilder
    {
        #region Substitute

        public static MethodCallExpression TrySubstituteWithOptimized(this MethodCallExpression node, string methodName, Func<MethodCallExpression, Expression> visitMethod)
        {
            if (node.Arguments.Count > 0 && node.Arguments[0] is MethodCallExpression methodCallExpression)
            {
                Expression? optimizedArgument = visitMethod(methodCallExpression);
                node = Expression.Call(typeof(Queryable), methodName,
                    node.Method.GetGenericArguments(), optimizedArgument);
            }

            return node;
        }
        
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

            Type selectorType = selectorNode.GetSelectorType();
            Type[] genericArgumentTypes = node.Method
                .GetGenericArguments()
                .Append(selectorType)
                .ToArray();

            return Expression.Call(typeof(Queryable), nameof(Queryable.Select), genericArgumentTypes, node.Arguments[0], selectorNode.Arguments[1]);
        }

        public static MethodCallExpression SubstituteWithWhere(this MethodCallExpression node, ExpressionVisitor optimizer, bool negate = false)
        {
            Check.NotNull(node, nameof(node));

            Expression predicate = node.Arguments[1];

            if (negate)
            {
                LambdaExpression lambdaExpression = node.GetLambda();
                UnaryExpression body = Expression.Not(lambdaExpression.Body);
                predicate = body.WrapInLambda(lambdaExpression.Parameters);
            }

            MethodCallExpression e = Expression.Call(typeof(Queryable), nameof(Queryable.Where), node.Method.GetGenericArguments(), node.Arguments[0], predicate);
            return (MethodCallExpression)optimizer.Visit(e);
        }

        #endregion

        #region Wrap

        public static MethodCallExpression WrapInTake(this MethodCallExpression node, int numberOfElements)
        {
            Check.NotNull(node, nameof(node));

            return Expression.Call(typeof(Queryable), nameof(Queryable.Take),
                node.Method.GetGenericArguments().Take(1).ToArray(), node, Expression.Constant(numberOfElements));
        }

        public static MethodCallExpression WrapInSelect(this MethodCallExpression node, MethodCallExpression selectorNode)
        {
            Check.NotNull(node, nameof(node));

            Type selectorType = selectorNode.GetSelectorType();
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

            Type selectorType = wrap.GetSelectorType();
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

        public static MethodCallExpression WrapInDiscriminatorFilter<TSource>(this Expression node, string discriminator)
            where TSource : CouchDocument
        {
            Check.NotNull(node, nameof(node));

            Expression<Func<TSource, bool>> filter = (d) => d.SplitDiscriminator == discriminator;

            return Expression.Call(typeof(Queryable), nameof(Queryable.Where),
                new[] { typeof(TSource) }, node, filter);
        }

        #endregion
    }
}