using System.Linq.Expressions;

namespace CouchDB.Driver.Query.Extensions
{
    internal static class ExpressionExtensions
    {        
        public static bool IsTrue(this Expression expression)
        {
            return expression is ConstantExpression c && c.Value is bool b && b;
        }

        public static bool IsFalse(this Expression expression)
        {
            return expression is ConstantExpression c && c.Value is bool b && !b;
        }
    }
}
