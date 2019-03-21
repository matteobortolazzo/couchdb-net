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
                    sb.Append("{\"$not\":");
                    this.Visit(u.Operand);
                    sb.Append("}");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }
            return u;
        }

    }
}
