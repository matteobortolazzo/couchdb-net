using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using CouchDB.Client.Query.Extensions;
using CouchDB.Client.Query.Selector.Nodes;

namespace CouchDB.Client.Query.Selector
{
    internal static class SelectorObjectBuilder
    {
        internal static ExpandoObject Serialize<T>(Expression<Func<T, bool>> predicate) where T : CouchEntity
        {
            var node = SelectorNodesBuilder<T>.GetQueryNodes(predicate);
            var optimizedNode = SelectorNodesOptimizer.Optimize(node);
            var nodeObject = BuildObjectNode(optimizedNode);
            return OptimizeAndOperators(nodeObject);
        }

        private static ExpandoObject OptimizeAndOperators(ExpandoObject node)
        {
            var nodeDictioray = (IDictionary<string, object>)node;
            var andChildren = nodeDictioray.Where(n => n.Key == "$and").ToList();
            if (!andChildren.Any())
                return (ExpandoObject)nodeDictioray;

            foreach (var andChild in andChildren)
            {
                var andSubChildren = andChild.Value as IEnumerable<IDictionary<string, object>>;

                if (andSubChildren == null) continue;

                // If both conditions are off the same property, don't combine them
                var keys = andSubChildren.SelectMany(list => list.Select(kvp => kvp.Key)).ToArray();
                if (keys.Length != keys.Distinct().Count())
                {
                    continue;
                }

                nodeDictioray.Remove(andChild);
                foreach (var andSubChild in andSubChildren)
                {
                    nodeDictioray.Add(andSubChild.First().Key, andSubChild.First().Value);
                }
            }

            return (ExpandoObject)nodeDictioray;
        }

        private static ExpandoObject BuildObjectNode(ICouchNode node)
        {
            switch (node)
            {
                case ConditionNode conditionNode:
                    {
                        if (conditionNode.Type == ConditionNodeType.Equal)
                        {
                            var condition = new ExpandoObject();
                            condition.AddProperty(conditionNode.PropertyNode.Name, conditionNode.ValueNode.Value);
                            return condition;
                        }

                        var symbol = GetConditionSymbol(conditionNode);

                        var rightConditionObject = new ExpandoObject();
                        rightConditionObject.AddProperty(symbol, conditionNode.ValueNode.Value);

                        var conditionObject = new ExpandoObject();
                        conditionObject.AddProperty(conditionNode.PropertyNode.Name, rightConditionObject);

                        return conditionObject;
                    }
                case UnitaryNode unitaryNode:
                    {
                        var childObject = BuildObjectNode(unitaryNode.Child);

                        var symbol = GetUnitarySymbol(unitaryNode.Type);

                        var unitaryObject = new ExpandoObject();
                        unitaryObject.AddProperty(symbol, childObject);

                        return unitaryObject;
                    }
                case CombinationNode combinationNode:
                    {
                        var symbol = GetCombinationSymbol(combinationNode.Type);

                        var childrenObjects = combinationNode.Children.Select(c => BuildObjectNode(c)).ToList();

                        var combinationObject = new ExpandoObject();
                        combinationObject.AddProperty(symbol, childrenObjects);

                        return combinationObject;
                    }
                case ArrayMatchNode arrayMatchNode:
                    {
                        var symbol = GetArrayMatchSymbol(arrayMatchNode.Type);

                        var arraySelectorNode = BuildObjectNode(arrayMatchNode.ArraySelectorNode);

                        var arraySelectorObject = new ExpandoObject();
                        arraySelectorObject.AddProperty(symbol, arraySelectorNode);

                        var arrayMatchObject = new ExpandoObject();
                        arrayMatchObject.AddProperty(arrayMatchNode.PropertyNode.Name, arraySelectorObject);

                        return arrayMatchObject;
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

        private static string GetArrayMatchSymbol(ArrayMatchNodeType type)
        {
            switch (type)
            {
                case ArrayMatchNodeType.All:
                    return "$allMatch";
                case ArrayMatchNodeType.Any:
                    return "$elemMatch";
            }
            throw new IndexOutOfRangeException();
        }
    }
}
