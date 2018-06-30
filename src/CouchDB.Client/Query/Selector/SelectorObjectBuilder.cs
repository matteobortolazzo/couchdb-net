using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using CouchDB.Client.Query.Selector.Nodes;

namespace CouchDB.Client.Query.Selector
{
    internal static class SelectorObjectBuilder
    {
        internal static dynamic Serialize<T>(Expression<Func<T, bool>> predicate) where T : CouchEntity
        {
            var node = SelectorNodesBuilder<T>.GetQueryNodes(predicate);
            var optimizedNodde = SelectorNodesOptimizer.Optimize(node);
            return BuildObjectNode(optimizedNodde);
        }

        private static dynamic BuildObjectNode(ICouchNode node)
        {
            switch (node)
            {
                case ConditionNode conditionNode:
                {
                    if (conditionNode.Type == ConditionNodeType.Equal)
                    {
                        var condition = new ExpandoObject();
                        AddProperty(condition, conditionNode.PropertyNode.Name, conditionNode.ValueNode.Value);
                        return condition;
                    }

                    var symbol = GetConditionSymbol(conditionNode);

                    var rightConditionObject = new ExpandoObject();
                    AddProperty(rightConditionObject, symbol, conditionNode.ValueNode.Value);

                    var conditionObject = new ExpandoObject();
                    AddProperty(conditionObject, conditionNode.PropertyNode.Name, rightConditionObject);

                    return conditionObject;
                }
                case UnitaryNode unitaryNode:
                {
                    var childObject = BuildObjectNode(unitaryNode.Child);

                    var symbol = GetUnitarySymbol(unitaryNode.Type);

                    var unitaryObject = new ExpandoObject();
                    AddProperty(unitaryObject, symbol, childObject);

                    return unitaryObject;
                }
                case CombinationNode combinationNode:
                {
                    var symbol = GetCombinationSymbol(combinationNode.Type);

                    var childrenObjects = combinationNode.Children.Select(c => BuildObjectNode(c)).ToList();

                    var combinationObject = new ExpandoObject();
                    AddProperty(combinationObject, symbol, childrenObjects);

                    return combinationObject;
                }
            }
            throw new InvalidOperationException();
        }

        private static string GetConditionSymbol(ConditionNode c)
        {
            switch (c.Type)
            {
                case ConditionNodeType.NotEqual:
                    return "$ne";
                case ConditionNodeType.GreaterThan:
                    return "$gt";
                case ConditionNodeType.GreaterThanOrEqual:
                    return "$gte";
                case ConditionNodeType.LessThan:
                    return "$lt";
                case ConditionNodeType.LessThanOrEqual:
                    return "$lte";
            }
            throw new IndexOutOfRangeException();
        }

        private static string GetUnitarySymbol(UnitaryNodeType type)
        {
            switch (type)
            {
                case UnitaryNodeType.Not:
                    return "$not";
            }
            throw new IndexOutOfRangeException();
        }

        private static string GetCombinationSymbol(CombinationNodeType type)
        {
            switch (type)
            {
                case CombinationNodeType.And:
                    return "$and";
                case CombinationNodeType.Or:
                    return "$or";
            }
            throw new IndexOutOfRangeException();
        }

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            // ExpandoObject supports IDictionary so we can extend it like this
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}
