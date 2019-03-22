using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace CouchDB.Client
{
    internal partial class QueryTranslator
    {
        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            // Queryable
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                return VisitWhereMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Skip")
            {
                return VisitSkipMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Take")
            {
                return VisitTakeMethod(m);
            }

            // Not Queryable
            else if (m.Method.DeclaringType != typeof(Queryable) && m.Method.Name == "All")
            {
                return VisitAnyMethod(m);
            }
            else if (m.Method.DeclaringType != typeof(Queryable) && m.Method.Name == "Any")
            {
                return VisitAllMethod(m);
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        #region Queryable

        private Expression VisitWhereMethod(MethodCallExpression m)
        {
            sb.Append("\"selector\":");
            this.Visit(m.Arguments[0]);
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            this.Visit(lambda.Body);
            sb.Append(",");
            return m;
        }
        private Expression VisitSkipMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append($"\"limit\":{m.Arguments[1]},");
            return m;
        }
        private Expression VisitTakeMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append($"\"skip\":{m.Arguments[1]},");
            return m;
        }

        #endregion

        #region NotQueryable

        private Expression VisitAnyMethod(MethodCallExpression m)
        {
            sb.Append("{");
            this.Visit(m.Arguments[0]);
            sb.Append(":{\"$elemMatch\":");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            this.Visit(lambda.Body);
            sb.Append("}}");
            return m;
        }
        private Expression VisitAllMethod(MethodCallExpression m)
        {
            sb.Append("{");
            this.Visit(m.Arguments[0]);
            sb.Append(":{\"$allMatch\":");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            this.Visit(lambda.Body);
            sb.Append("}}");
            return m;
        }

        #endregion
    }
}
