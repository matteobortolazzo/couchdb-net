using System;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitBinary(BinaryExpression b)
        {
            sb.Append("{");
            if (b.NodeType == ExpressionType.Equal)
            {
                // $size operator = array.Count == size
                if (b.Left is MemberExpression m && m.Member.Name == "Count")
                {
                    this.Visit(m.Expression);
                    sb.Append(":{\"$size\":");
                    this.Visit(b.Right);
                    sb.Append("}}");
                    return b;
                }
                // $mod operator = prop % divisor = remainder 
                else if (b.Left is BinaryExpression mb && mb.NodeType == ExpressionType.Modulo)
                {
                    if (mb.Left is MemberExpression c && c.Member is PropertyInfo r && r.PropertyType == typeof(int))
                    {
                        this.Visit(mb.Left);
                        sb.Append(":{\"$mod\":[");
                        this.Visit(mb.Right);
                        sb.Append(",");
                        this.Visit(b.Right);
                        sb.Append("]}}");
                        return b;
                    }
                    else
                        throw new NotSupportedException($"The document field must be an integer.");
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

        private void VisitBinaryCombinationOperator(BinaryExpression b, bool not = false)
        {
            void InspectBinaryChildren(BinaryExpression e, ExpressionType nodeType)
            {
                if (e.Left is BinaryExpression lb && lb.NodeType == nodeType)
                {
                    InspectBinaryChildren(lb, nodeType);
                    sb.Append(",");
                    this.Visit(e.Right);
                    return;
                }

                if (e.Right is BinaryExpression rb && rb.NodeType == nodeType)
                {
                    this.Visit(e.Left);
                    sb.Append(",");
                    InspectBinaryChildren(rb, nodeType);
                    return;
                }

                this.Visit(e.Left);
                sb.Append(",");
                this.Visit(e.Right);
            }

            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sb.Append("\"$and\":[");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    if (not)
                        sb.Append("\"$nor\":[");
                    else
                        sb.Append("\"$or\":[");
                    break;
            }

            InspectBinaryChildren(b, b.NodeType);
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
