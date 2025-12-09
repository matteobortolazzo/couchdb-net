using System.Linq.Expressions;

namespace CouchDB.Driver.Query.Extensions;

internal static class ExpressionExtensions
{
    extension(Expression expression)
    {
        public bool IsTrue()
        {
            return expression is ConstantExpression { Value: true };
        }

        public bool IsFalse()
        {
            return expression is ConstantExpression { Value: false };
        }

        public bool IsBoolean()
        {
            return expression is ConstantExpression { Value: bool };
        }
    }
}