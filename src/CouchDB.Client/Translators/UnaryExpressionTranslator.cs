using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CouchDB.Client
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    // $nin operator with single value = !Contains(value)
                    if (u.Operand is MethodCallExpression m && m.Method.Name == "Contains")
                    {
                        sb.Append("{");
                        this.Visit(m.Object);
                        sb.Append(":{\"$nin\":[");
                        this.Visit(m.Arguments[0]);
                        sb.Append("]}}");
                    }
                    else
                    {
                        sb.Append("{\"$not\":");
                        this.Visit(u.Operand);
                        sb.Append("}");
                    }
                    break;
                case ExpressionType.Convert:
                    this.Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

    }
}
