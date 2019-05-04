using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver.ExpressionVisitors
{
    internal  class WhereExpressionVisitor : ExpressionVisitor
    {
        private bool _visitingWhereMethod;

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            _visitingWhereMethod = m.Method.Name == "Where" && m.Method.DeclaringType == typeof(Queryable);
            if (_visitingWhereMethod)
            {
                Expression result = base.VisitMethodCall(m);
                _visitingWhereMethod = false;
                return result;
            }
            return base.VisitMethodCall(m);
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            if (expression.Right is ConstantExpression c && c.Type == typeof(bool) && 
                (expression.NodeType == ExpressionType.Equal || expression.NodeType == ExpressionType.NotEqual))
            {
                return expression;
            }
            return base.VisitBinary(expression);
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            if (IsWhereBooleanExpression(expression))
            {
                return Expression.MakeBinary(ExpressionType.Equal, expression, Expression.Constant(true));
            }
            return base.VisitMember(expression);
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Not && expression.Operand is MemberExpression m && IsWhereBooleanExpression(m))
            {
                return Expression.MakeBinary(ExpressionType.Equal, m, Expression.Constant(false));
            }
            return base.VisitUnary(expression);
        }

        private bool IsWhereBooleanExpression(MemberExpression expression)
        {
            return _visitingWhereMethod && 
                expression.Member is PropertyInfo info && 
                info.PropertyType == typeof(bool);
        }
    }
}
