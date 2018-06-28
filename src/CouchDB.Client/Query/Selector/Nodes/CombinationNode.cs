using System.Collections.Generic;

namespace CouchDB.Client.Query.Selector.Nodes
{
    internal enum CombinationNodeType
    {
        And, Or
    }

    internal class CombinationNode : ICouchNode
    {
        public CombinationNodeType Type { get; set; }
        public List<ICouchNode> Children { get; set; }
    }
}
