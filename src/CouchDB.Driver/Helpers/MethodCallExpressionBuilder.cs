using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CouchDB.Driver.Extensions;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Helpers;

internal static class MethodCallExpressionBuilder
{
    #region Substitute

    extension(MethodCallExpression node)
    {
        public MethodCallExpression TrySubstituteWithOptimized(string methodName,
            Func<MethodCallExpression, Expression> visitMethod)
        {
            if (node.Arguments.Count > 0 && node.Arguments[0] is MethodCallExpression methodCallExpression)
            {
                Expression optimizedArgument = visitMethod(methodCallExpression);
                node = Expression.Call(typeof(Queryable), methodName,
                    node.Method.GetGenericArguments(), optimizedArgument);
            }

            return node;
        }

        public MethodCallExpression SubstituteWithQueryableCall(string methodName)
        {
            ArgumentNullException.ThrowIfNull(node);

            return Expression.Call(typeof(Queryable), methodName, node.Method.GetGenericArguments(), node.Arguments[0],
                node.Arguments[1]);
        }

        public MethodCallExpression SubstituteWithTake(int numberOfElements)
        {
            ArgumentNullException.ThrowIfNull(node);

            return Expression.Call(typeof(Queryable), nameof(Queryable.Take),
                node.Method.GetGenericArguments().Take(1).ToArray(), node.Arguments[0],
                Expression.Constant(numberOfElements));
        }

        public MethodCallExpression SubstituteWithSelect(MethodCallExpression selectorNode)
        {
            ArgumentNullException.ThrowIfNull(node);

            Type selectorType = selectorNode.GetSelectorType();
            Type[] genericArgumentTypes = node.Method
                .GetGenericArguments()
                .Append(selectorType)
                .ToArray();

            return Expression.Call(typeof(Queryable), nameof(Queryable.Select), genericArgumentTypes, node.Arguments[0],
                selectorNode.Arguments[1]);
        }

        public MethodCallExpression SubstituteWithWhere(ExpressionVisitor optimizer, bool negate = false)
        {
            ArgumentNullException.ThrowIfNull(node);

            Expression predicate = node.Arguments[1];

            if (negate)
            {
                LambdaExpression lambdaExpression = node.GetLambda();
                UnaryExpression body = Expression.Not(lambdaExpression.Body);
                predicate = body.WrapInLambda(lambdaExpression.Parameters);
            }

            MethodCallExpression e = Expression.Call(typeof(Queryable), nameof(Queryable.Where),
                node.Method.GetGenericArguments(), node.Arguments[0], predicate);
            return (MethodCallExpression)optimizer.Visit(e);
        }
    }

    #endregion

    #region Wrap

    extension(MethodCallExpression node)
    {
        public MethodCallExpression WrapInTake(int numberOfElements)
        {
            ArgumentNullException.ThrowIfNull(node);

            return Expression.Call(typeof(Queryable), nameof(Queryable.Take),
                node.Method.GetGenericArguments().Take(1).ToArray(), node, Expression.Constant(numberOfElements));
        }

        public MethodCallExpression WrapInSelect(MethodCallExpression selectorNode)
        {
            ArgumentNullException.ThrowIfNull(node);

            Type selectorType = selectorNode.GetSelectorType();
            Type[] genericArgumentTypes = node.Method
                .GetGenericArguments()
                .Append(selectorType)
                .ToArray();

            return Expression.Call(typeof(Queryable), nameof(Queryable.Select), genericArgumentTypes, node,
                selectorNode.Arguments[1]);
        }

        public MethodCallExpression WrapInAverageSum(MethodCallExpression wrap)
        {
            ArgumentNullException.ThrowIfNull(node);
            ArgumentNullException.ThrowIfNull(wrap);

            Type selectorType = wrap.GetSelectorType();
            Type queryableType = typeof(IQueryable<>).MakeGenericType(selectorType);
            MethodInfo numberMethod = typeof(Queryable).GetMethod(wrap.Method.Name, [queryableType])!;
            return Expression.Call(numberMethod, node);
        }

        public MethodCallExpression WrapInMinMax(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(node);
            ArgumentNullException.ThrowIfNull(methodInfo);

            MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(node.Method.GetGenericArguments()[1]);
            return Expression.Call(genericMethodInfo, node);
        }

        public MethodCallExpression WrapInMethodWithoutSelector(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(node);
            ArgumentNullException.ThrowIfNull(methodInfo);

            MethodInfo genericMethodInfo = methodInfo.MakeGenericMethod(node.Method.GetGenericArguments()[0]);
            return Expression.Call(genericMethodInfo, node);
        }
    }

    public static MethodCallExpression WrapInDiscriminatorFilter<TSource>(this Expression node, string discriminator)
        where TSource : CouchDocument
    {
        ArgumentNullException.ThrowIfNull(node);

        Expression<Func<TSource, bool>> filter = (d) => d.SplitDiscriminator == discriminator;

        return Expression.Call(typeof(Queryable), nameof(Queryable.Where),
            [typeof(TSource)], node, filter);
    }

    #endregion
}