using System;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable IDE0058 // Expression value is never used
namespace CouchDB.Driver.Query
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitBinary(BinaryExpression b)
        {
            _sb.Append('{');
            switch (b.NodeType)
            {
                case ExpressionType.Equal:
                    switch (b.Left)
                    {
                        // $size operator = array.Count == size
                        // $mod operator = prop % divisor = remainder 
                        case MemberExpression m when m.Member.Name == "Count":
                            Visit(m.Expression);
                            _sb.Append(":{\"$size\":");
                            Visit(b.Right);
                            _sb.Append("}}");
                            return b;
                        case BinaryExpression mb when mb.NodeType == ExpressionType.Modulo:
                        {
                            if (!(mb.Left is MemberExpression c) || !(c.Member is PropertyInfo r) ||
                                r.PropertyType != typeof(int))
                            {
                                throw new NotSupportedException($"The document field must be an integer.");
                            }

                            Visit(mb.Left);
                            _sb.Append(":{\"$mod\":[");
                            Visit(mb.Right);
                            _sb.Append(',');
                            Visit(b.Right);
                            _sb.Append("]}}");
                            return b;

                        }
                        default:
                            Visit(b.Left);
                            _sb.Append(':');
                            Visit(b.Right);
                            break;
                    }

                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    VisitBinaryCombinationOperator(b);
                    break;
                default:
                    VisitBinaryConditionOperator(b);
                    break;
            }
            _sb.Append('}');
            return b;
        }

        private void VisitBinaryCombinationOperator(BinaryExpression b, bool not = false)
        {
            void InspectBinaryChildren(BinaryExpression e, ExpressionType nodeType)
            {
                if (e.Left is BinaryExpression lb && lb.NodeType == nodeType)
                {
                    InspectBinaryChildren(lb, nodeType);
                    _sb.Append(',');
                    Visit(e.Right);
                    return;
                }

                if (e.Right is BinaryExpression rb && rb.NodeType == nodeType)
                {
                    Visit(e.Left);
                    _sb.Append(',');
                    InspectBinaryChildren(rb, nodeType);
                    return;
                }

                Visit(e.Left);
                _sb.Append(',');
                Visit(e.Right);
            }

            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _sb.Append("\"$and\":[");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    _sb.Append(not ? "\"$nor\":[" : "\"$or\":[");
                    break;
            }

            InspectBinaryChildren(b, b.NodeType);
            _sb.Append(']');
        }

        private void VisitBinaryConditionOperator(BinaryExpression b)
        {
            Visit(b.Left);
            _sb.Append(":{");

            switch (b.NodeType)
            {
                case ExpressionType.NotEqual:
                    _sb.Append("\"$ne\":");
                    break;
                case ExpressionType.LessThan:
                    _sb.Append("\"$lt\":");
                    break;
                case ExpressionType.LessThanOrEqual:
                    _sb.Append("\"$lte\":");
                    break;
                case ExpressionType.GreaterThan:
                    _sb.Append("\"$gt\":");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _sb.Append("\"$gte\":");
                    break;
                default:
                    throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
            }

            Visit(b.Right);
            _sb.Append('}');
        }
    }
}
#pragma warning restore IDE0058 // Expression value is never used