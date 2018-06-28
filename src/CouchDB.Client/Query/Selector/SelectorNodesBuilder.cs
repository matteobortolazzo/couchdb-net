using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
            if (expr is MemberExpression m)
            {
                var jsonPropertyAttributes = m.Member.GetCustomAttributes(typeof(JsonPropertyAttribute), true);
                var jsonProperty = jsonPropertyAttributes.Length > 0 ? jsonPropertyAttributes[0] as JsonPropertyAttribute : null;

                var propName = jsonProperty != null ? jsonProperty.PropertyName : m.Member.Name;

                return new MemberNode { Name = propName };
            }
            if (expr is ConstantExpression s)
            {
                return new ConstantNode { Value = s.Value };
            }
            if (expr is BinaryExpression b)
            {
                var right = NewCouchNode(b.Right);
                var left = NewCouchNode(b.Left);

                if (IsCombinationNode(b.NodeType))
                    return NewCombinationNode(b.NodeType, right, left);

                if (IsConditionNode(b.NodeType))
                    return NewConditionNode(b.NodeType, (MemberNode)left, (ConstantNode)right);

                throw new NotImplementedException();
            }
            if (expr is UnaryExpression u)
            {
                if(u.NodeType == ExpressionType.Not)
                {
                    var op = NewCouchNode(u.Operand);
                    return NewUnitaryNoder(u.NodeType, op);
                }
            }
            //else if (expr is ParameterExpression p) { }
            //else if (expr is MethodCallExpression c) { }
            //else if (expr is LambdaExpression l) { }
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
                switch(type)
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

        private static ICouchNode NewUnitaryNoder(ExpressionType type, ICouchNode childOperator)
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
