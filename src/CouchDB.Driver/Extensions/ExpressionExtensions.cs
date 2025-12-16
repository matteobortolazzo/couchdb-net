using System.Linq.Expressions;
using System.Text.Json;
using CouchDB.Driver.Options;

namespace CouchDB.Driver.Extensions;

internal static class ExpressionExtensions
{
    public static string GetPropertyName(this MemberExpression m, CouchOptions options)
    {
        JsonNamingPolicy caseType = options.JsonSerializerOptions?.PropertyNamingPolicy ?? JsonNamingPolicy.CamelCase;

        var members = new List<string> { m.Member.GetCouchPropertyName(caseType) };

        Expression? currentExpression = m.Expression;

        if (currentExpression is MemberExpression rootMemberExpression && rootMemberExpression.Type.IsEnumerable())
        {
            throw new NotSupportedException();
        }

        while (currentExpression is MemberExpression cm)
        {
            members.Add(cm.Member.GetCouchPropertyName(caseType));
            currentExpression = cm.Expression;
        }

        members.Reverse();
        var propName = string.Join(".", members.ToArray());

        return propName;
    }

    extension(Expression expression)
    {
        public bool ContainsSelector() =>
            expression is MethodCallExpression { Arguments.Count: 2 } m && m.Arguments[1].IsSelectorExpression();

        private bool IsSelectorExpression() =>
            expression is UnaryExpression { Operand: LambdaExpression { Body: MemberExpression } };

        public Expression WrapInLambda(IReadOnlyCollection<ParameterExpression> lambdaParameters)
        {
            LambdaExpression lambdaExpression = Expression.Lambda(expression, lambdaParameters);
            return Expression.Quote(lambdaExpression);
        }

        private Expression StripQuotes()
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

        public MemberExpression ToMemberExpression()
        {
            if (expression is not LambdaExpression { Body: MemberExpression m })
            {
                throw new ArgumentException("The given expression does not select a property.", nameof(expression));
            }

            return m;
        }
    }

    extension(MethodCallExpression selector)
    {
        public Type GetSelectorType() =>
            selector.Arguments[1] is UnaryExpression { Operand: LambdaExpression { Body: MemberExpression m } }
                ? m.Type
                : throw new InvalidOperationException(
                    $"Method {selector.Method.Name} does not select a property.");

        public Delegate GetSelectorDelegate() =>
            selector.Arguments[1] is UnaryExpression { Operand: LambdaExpression l }
                ? l.Compile()
                : throw new InvalidOperationException(
                    $"Method {selector.Method.Name} does not select a property.");

        public Expression GetLambdaBody()
            => selector.GetLambda().Body;

        public LambdaExpression GetLambda()
        {
            return (LambdaExpression)selector.Arguments[1].StripQuotes();
        }
    }

    public static MethodCallExpression WrapInWhereExpression<TSource>(this Expression<Func<TSource, bool>> selector)
    {
        MethodCallExpression whereExpression = Expression.Call(typeof(Queryable), nameof(Queryable.Where),
            [typeof(TSource)], Expression.Constant(Array.Empty<TSource>().AsQueryable()), selector);
        return whereExpression;
    }
}