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
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                sb.Append("{\"selector\":");
                this.Visit(m.Arguments[0]);
                LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                this.Visit(lambda.Body);
                sb.Append("}");
                return m;
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }
    }
}
