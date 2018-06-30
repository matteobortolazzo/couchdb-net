using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouchDB.Client.Query.Selector.Nodes;

namespace CouchDB.Client.Query.Selector
{
    internal static class SelectorNodesOptimizer
    {
        internal static ICouchNode Optimize(ICouchNode node)
        {
            var result =  DoOptimization(node);
            if (result.IsOptimizable)
                ApplyOptimization((CombinationNode)result.Node);
            return result.Node;
        }

        private static (ICouchNode Node, bool IsOptimizable) DoOptimization(ICouchNode node)
        {
            if (!(node is CombinationNode))
                return (node, false);

            var currentNode = (CombinationNode) node;
            
            var optimizableChildren = currentNode.Children.Select(DoOptimization).ToList();
            
            foreach (var child in optimizableChildren)
            {
                if (!child.IsOptimizable) continue;
                ApplyOptimization((CombinationNode) child.Node);
            }

            var optimizable = optimizableChildren.All(c =>
            {
                if (!(c.Node is CombinationNode)) return true;
                return ((CombinationNode)c.Node).Type == currentNode.Type;
            });

            return (currentNode, optimizable);
        }

        private static ICouchNode ApplyOptimization(CombinationNode node)
        {
            var grandchildren = new List<ICouchNode>();

            foreach (var grandchild in node.Children)
            {
                if (grandchild is CombinationNode combinationNode)
                    grandchildren.AddRange(combinationNode.Children);
                else
                    grandchildren.Add(grandchild);
            }

            node.Children.Clear();
            node.Children.AddRange(grandchildren);

            return node;
        }
    }
}
