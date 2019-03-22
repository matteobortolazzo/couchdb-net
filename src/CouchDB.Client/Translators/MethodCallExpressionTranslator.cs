using System;
using System.Linq;
using System.Linq.Expressions;

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
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderBy")
            {
                return VisitOrderByMethod(m, true);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "ThenBy")
            {
                return VisitOrderByMethod(m, true);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderByDescending")
            {
                return VisitOrderByMethod(m, false);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "ThenByDescending")
            {
                return VisitOrderByMethod(m, false);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Skip")
            {
                return VisitSkipMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Take")
            {
                return VisitTakeMethod(m);
            }
            else if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Select")
            {
                return VisitSelectMethod(m);
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

        private Expression VisitOrderByMethod(MethodCallExpression m, bool ascending)
        {
            void InspectOrdering(Expression e)
            {
                var o = e as MethodCallExpression;
                if (o.Method.DeclaringType != typeof(Queryable))
                    throw new NotSupportedException(string.Format("The method '{0}' is not supported", o.Method.Name));
                
                var lambda = (LambdaExpression)StripQuotes(o.Arguments[1]);
                if (o.Method.Name == "OrderBy")
                {
                    this.Visit(o.Arguments[0]);
                    if (!ascending)
                        throw new InvalidOperationException("Cannot order in different directions.");
                    sb.Append("\"sort\":[");
                    this.Visit(lambda.Body);
                }
                else if (o.Method.Name == "OrderByDescending")
                {
                    this.Visit(o.Arguments[0]);
                    if (ascending)
                        throw new InvalidOperationException("Cannot order in different directions.");
                    sb.Append("\"sort\":[");
                    sb.Append("{");
                    this.Visit(lambda.Body);
                    sb.Append(":\"desc\"}");
                }
                else if (o.Method.Name == "ThenBy")
                {
                    if (!ascending)
                        throw new InvalidOperationException("Cannot order in different directions.");
                    InspectOrdering(o.Arguments[0]);
                    this.Visit(lambda.Body);
                }
                else if (o.Method.Name == "ThenByDescending")
                {
                    if (ascending)
                        throw new InvalidOperationException("Cannot order in different directions.");
                    InspectOrdering(o.Arguments[0]);
                    sb.Append("{");
                    this.Visit(lambda.Body);
                    sb.Append(":\"desc\"}");
                }
                else
                {
                    return;
                }
                sb.Append(",");
            }

            InspectOrdering(m);
            sb.Append("],");
            return m;
        }

        #region Queryable

        private Expression VisitWhereMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append("\"selector\":");
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
        private Expression VisitSelectMethod(MethodCallExpression m)
        {
            this.Visit(m.Arguments[0]);
            sb.Append("\"fields\":[");
            var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
            var n = lambda.Body as NewExpression;

            if (n == null)
                throw new NotSupportedException($"The expression of type {lambda.Body.GetType()} is not supported in the Select method.");

            foreach (var a in n.Arguments)
            {
                this.Visit(a);
                sb.Append(",");
            }
            sb.Append("],");

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
