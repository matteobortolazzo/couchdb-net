using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CouchDB.Client
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("{");
            if (b.NodeType == ExpressionType.Equal)
            {
                if (b.Left is MemberExpression m && m.Member.Name == "Count")
                {
                    this.Visit(m.Expression);
                    sb.Append(":{\"$size\":");
                    this.Visit(b.Right);
                    sb.Append("}}");
                    return m;
                }
                else
                {
                    this.Visit(b.Left);
                    sb.Append(":");
                    this.Visit(b.Right);
                }
            }
            else if (b.NodeType == ExpressionType.And ||
                b.NodeType == ExpressionType.AndAlso ||
                b.NodeType == ExpressionType.Or ||
                b.NodeType == ExpressionType.OrElse)
            {
                VisitBinaryCombinationOperator(b);
            }
            else
            {
                VisitBinaryConditionOperator(b);
            }
            sb.Append("}");
            return b;
        }

        private void VisitBinaryCombinationOperator(BinaryExpression b)
        {
            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sb.Append("\"$and\":[");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sb.Append("\"$or\":[");
                    break;
            }

            void InspectChildren(BinaryExpression e)
            {
                if (e.Left is BinaryExpression lb && lb.NodeType == b.NodeType)
                {
                    InspectChildren(lb);
                    sb.Append(",");
                    this.Visit(e.Right);
                    return;
                }

                if (e.Right is BinaryExpression rb && rb.NodeType == b.NodeType)
                {
                    this.Visit(e.Left);
                    sb.Append(",");
                    InspectChildren(rb);
                    return;
                }

                this.Visit(e.Left);
                sb.Append(",");
                this.Visit(e.Right);
            }

            InspectChildren(b);
            sb.Append("]");
        }

        private void VisitBinaryConditionOperator(BinaryExpression b)
        {
            this.Visit(b.Left);
            sb.Append(":{");

            switch (b.NodeType)
            {
                case ExpressionType.NotEqual:
                    sb.Append("\"$ne\":");
                    break;
                case ExpressionType.LessThan:
                    sb.Append("\"$lt\":");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append("\"$lte\":");
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append("\"$gt\":");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append("\"$gte\":");
                    break;
                default:
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }

            this.Visit(b.Right);
            sb.Append("}");
        }
    }
}
