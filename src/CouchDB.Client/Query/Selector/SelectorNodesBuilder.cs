using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CouchDB.Client.Query.Selector.Nodes;
using Newtonsoft.Json;

namespace CouchDB.Client.Query.Selector
{
    internal static class SelectorNodesBuilder<T> where T : CouchEntity
    {
        internal static ICouchNode GetQueryNodes(Expression<Func<T, bool>> predicate)
        {
            return NewCouchNode(predicate.Body);
        }

        private static ICouchNode NewCouchNode(Expression expr)
        {
            if (expr is MemberExpression memberExpr)
            {
                string GetPropertyName(MemberInfo memberInfo)
                {
                    var jsonPropertyAttributes = memberInfo.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                    var jsonProperty = jsonPropertyAttributes.Length > 0 ? jsonPropertyAttributes[0] as JsonPropertyAttribute : null;

                    return jsonProperty != null ? jsonProperty.PropertyName : memberInfo.Name;
                }

                var members = new List<string>();

                Expression currentExpression = memberExpr;
                while (currentExpression is MemberExpression cm)
                {
                    members.Add(GetPropertyName(cm.Member));
                    currentExpression = cm.Expression;
                }
                if (currentExpression.NodeType == ExpressionType.Parameter)
                {
                    members.Reverse();
                    var propName = string.Join(".", members.ToArray());

                    return new MemberNode { Name = propName };
                }
                else if (currentExpression.NodeType == ExpressionType.Constant)
                {
                    var value = Expression.Lambda(memberExpr).Compile().DynamicInvoke();
                    return new ConstantNode() { Value = value };
                }
                else
                {
                    throw new NotImplementedException($"Expression.NodeType {currentExpression.NodeType} is not implemented");
                }
            }
            if (expr is ConstantExpression constantExpr)
            {
                return new ConstantNode { Value = constantExpr.Value };
            }
            if (expr is BinaryExpression binaryExpr)
            {
                var left = NewCouchNode(binaryExpr.Left);
                var right = NewCouchNode(binaryExpr.Right);

                if (IsCombinationNode(binaryExpr.NodeType))
                    return NewCombinationNode(binaryExpr.NodeType, right, left);

                if (IsConditionNode(binaryExpr.NodeType))
                    return NewConditionNode(binaryExpr.NodeType, (MemberNode)left, (ConstantNode)right);

                throw new NotImplementedException();
            }
            if (expr is UnaryExpression unaryExpr)
            {
                if (unaryExpr.NodeType == ExpressionType.Not)
                {
                    var op = NewCouchNode(unaryExpr.Operand);
                    return NewUnitaryNode(unaryExpr.NodeType, op);
                }
                else if (unaryExpr.NodeType == ExpressionType.Convert && unaryExpr.Operand is MemberExpression)
                {
                    return NewCouchNode(unaryExpr.Operand);
                }
                throw new NotImplementedException();
            }
            //else if (expr is ParameterExpression p) { }
            if (expr is LambdaExpression lambdaExpr)
            {
                var lamdaNode = NewCouchNode(lambdaExpr.Body);
                return lamdaNode;
            }

            if (expr is MethodCallExpression methodExpr)
            {
                var argumentsNode = methodExpr.Arguments.Select(NewCouchNode).ToList();

                ArrayMatchNodeType type;
                switch (methodExpr.Method.Name)
                {
                    case "All":
                        type = ArrayMatchNodeType.All;
                        break;
                    case "Any":
                        type = ArrayMatchNodeType.Any;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                var arrayMatchNode = new ArrayMatchNode
                {
                    Type = type,
                    PropertyNode = (MemberNode)argumentsNode[0],
                    ArraySelectorNode = argumentsNode[1]
                };
                return arrayMatchNode;
            }

            throw new NotSupportedException($"Invalid Expression type: {expr.GetType()}");
        }

        #region Condition

        private static bool IsConditionNode(ExpressionType type) =>
            type == ExpressionType.Equal ||
            type == ExpressionType.NotEqual ||
            type == ExpressionType.GreaterThan ||
            type == ExpressionType.GreaterThanOrEqual ||
            type == ExpressionType.LessThan ||
            type == ExpressionType.LessThanOrEqual;

        private static ICouchNode NewConditionNode(ExpressionType type, MemberNode name, ConstantNode value)
        {
            ConditionNodeType GetType()
            {
                switch (type)
                {
                    case ExpressionType.Equal:
                        return ConditionNodeType.Equal;
                    case ExpressionType.NotEqual:
                        return ConditionNodeType.NotEqual;
                    case ExpressionType.GreaterThan:
                        return ConditionNodeType.GreaterThan;
                    case ExpressionType.GreaterThanOrEqual:
                        return ConditionNodeType.GreaterThanOrEqual;
                    case ExpressionType.LessThan:
                        return ConditionNodeType.LessThan;
                    case ExpressionType.LessThanOrEqual:
                        return ConditionNodeType.LessThanOrEqual;
                }
                throw new NotSupportedException($"Invalid condition expression type: {type.ToString()}");
            }
            return new ConditionNode
            {
                Type = GetType(),
                PropertyNode = name,
                ValueNode = value
            };
        }

        #endregion

        #region Combination

        private static bool IsCombinationNode(ExpressionType type) =>
            type == ExpressionType.AndAlso ||
            type == ExpressionType.OrElse;

        private static ICouchNode NewCombinationNode(ExpressionType type, ICouchNode right, ICouchNode left)
        {
            CombinationNodeType GetType()
            {
                switch (type)
                {
                    case ExpressionType.AndAlso:
                        return CombinationNodeType.And;
                    case ExpressionType.OrElse:
                        return CombinationNodeType.Or;
                }
                throw new NotSupportedException($"Invalid combination expression type: {type.ToString()}");
            }
            return new CombinationNode
            {
                Type = GetType(),
                Children = new List<ICouchNode>
                {
                    left,
                    right
                }
            };
        }

        #endregion

        #region Unitary

        private static bool IsUnitartyNode(ExpressionType type) =>
            type == ExpressionType.Not;

        private static ICouchNode NewUnitaryNode(ExpressionType type, ICouchNode childOperator)
        {
            UnitaryNodeType GetType()
            {
                switch (type)
                {
                    case ExpressionType.Not:
                        return UnitaryNodeType.Not;
                }
                throw new NotSupportedException($"Invalid unitary expression type: {type.ToString()}");
            }
            return new UnitaryNode
            {
                Type = GetType(),
                Child = childOperator
            };
        }

        #endregion
    }
}
