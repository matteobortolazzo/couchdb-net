using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CouchDB.Driver.Extensions
{
    internal static class ExpressionExtensions
    {
        public static bool ContainsSelector(this Expression expression) =>
            expression is MethodCallExpression m && m.Arguments.Count == 2 && m.Arguments[1].IsSelectorExpression();

        private static bool IsSelectorExpression(this Expression selector) =>
            selector is UnaryExpression u && u.Operand is LambdaExpression l && l.Body is MemberExpression;

        public static Type GetSelectorType(this MethodCallExpression selector) =>
            selector.Arguments[1] is UnaryExpression u && u.Operand is LambdaExpression l &&
            l.Body is MemberExpression m
                ? m.Type
                : throw new InvalidOperationException(
                    $"Method {selector.Method.Name} does not select a property.");

        public static Delegate GetSelectorDelegate(this MethodCallExpression selector) =>
            selector.Arguments[1] is UnaryExpression u && u.Operand is LambdaExpression l
                ? l.Compile()
                : throw new InvalidOperationException(
                    $"Method {selector.Method.Name} does not select a property.");

        public static Expression GetLambdaBody(this MethodCallExpression node)
            => node.GetLambda().Body;

        public static LambdaExpression GetLambda(this MethodCallExpression node)
        {
            return (LambdaExpression)node.Arguments[1].StripQuotes();
        }

        public static Expression WrapInLambda(this Expression body, IReadOnlyCollection<ParameterExpression> lambdaParameters)
        { 
            LambdaExpression lambdaExpression = Expression.Lambda(body, lambdaParameters);
            return Expression.Quote(lambdaExpression);
        }

        private static Expression StripQuotes(this Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
    }
}
