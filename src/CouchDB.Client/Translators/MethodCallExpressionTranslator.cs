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
                return VisitWhereMethod(m);
            }
            else if (m.Method.Name == "All")
            {
                if (m.Method.DeclaringType == typeof(Queryable))
                {
                    return VisitAnyQueryableMethod(m);
                }
                else
                {
                    return VisitAnyMethod(m);
                }
            }
            else if (m.Method.Name == "Any")
            {
                if (m.Method.DeclaringType == typeof(Queryable))
                {
                    return VisitAllQueryableMethod(m);
                }
                else
                {
                    return VisitAllMethod(m);
                }
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        #region Queryable

        private Expression VisitWhereMethod(MethodCallExpression m)
        {
            sb.Append("{\"selector\":");
            this.Visit(m.Arguments[0]);
            LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            this.Visit(lambda.Body);
            sb.Append("}");
            return m;
        }
        private Expression VisitAnyQueryableMethod(MethodCallExpression m)
        {
            throw new NotImplementedException();
        }
        private Expression VisitAllQueryableMethod(MethodCallExpression m)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region NotQueryable

        private Expression VisitAnyMethod(MethodCallExpression m)
        {
            throw new NotImplementedException();
        }
        private Expression VisitAllMethod(MethodCallExpression m)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
