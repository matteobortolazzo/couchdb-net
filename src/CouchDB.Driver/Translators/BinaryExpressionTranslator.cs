using System;
using System.Linq.Expressions;
using System.Reflection;

namespace CouchDB.Driver
{
    internal partial class QueryTranslator
    {
        protected override Expression VisitBinary(BinaryExpression b)
        {
            _sb.Append("{");
            if (b.NodeType == ExpressionType.Equal)
            {
                // $size operator = array.Count == size
                if (b.Left is MemberExpression m && m.Member.Name == "Count")
                {
                    Visit(m.Expression);
                    _sb.Append(":{\"$size\":");
                    Visit(b.Right);
                    _sb.Append("}}");
                    return b;
                }
                // $mod operator = prop % divisor = remainder 
                else if (b.Left is BinaryExpression mb && mb.NodeType == ExpressionType.Modulo)
                {
                    if (mb.Left is MemberExpression c && c.Member is PropertyInfo r && r.PropertyType == typeof(int))
                    {
                        Visit(mb.Left);
                        _sb.Append(":{\"$mod\":[");
                        Visit(mb.Right);
                        _sb.Append(",");
                        Visit(b.Right);
                        _sb.Append("]}}");
                        return b;
                    }
                    else
                    {
                        throw new NotSupportedException($"The document field must be an integer.");
                    }
                }
                else
                {
                    Visit(b.Left);
                    _sb.Append(":");
                    Visit(b.Right);
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
            _sb.Append("}");
            return b;
        }

        private void VisitBinaryCombinationOperator(BinaryExpression b, bool not = false)
        {
            void InspectBinaryChildren(BinaryExpression e, ExpressionType nodeType)
            {
                if (e.Left is BinaryExpression lb && lb.NodeType == nodeType)
                {
                    InspectBinaryChildren(lb, nodeType);
                    _sb.Append(",");
                    Visit(e.Right);
                    return;
                }

                if (e.Right is BinaryExpression rb && rb.NodeType == nodeType)
                {
                    Visit(e.Left);
                    _sb.Append(",");
                    InspectBinaryChildren(rb, nodeType);
                    return;
                }

                ForceToBinaryExpressionAndVisit(e.Left);
                _sb.Append(",");
                ForceToBinaryExpressionAndVisit(e.Right);
            }

            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _sb.Append("\"$and\":[");
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    if (not)
                    {
                        _sb.Append("\"$nor\":[");
                    }
                    else
                    {
                        _sb.Append("\"$or\":[");
                    }
                    break;
            }

            InspectBinaryChildren(b, b.NodeType);
            _sb.Append("]");
        }

        private void ForceToBinaryExpressionAndVisit(Expression expression)
        {
            if (expression is MemberExpression)
            {
                expression = Expression.MakeBinary(ExpressionType.Equal, expression, Expression.Constant(true));
            }
            Visit(expression);
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
            _sb.Append("}");
        }
    }
}
