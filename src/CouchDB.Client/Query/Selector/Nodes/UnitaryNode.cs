namespace CouchDB.Client.Query.Selector.Nodes
{
    internal enum UnitaryNodeType
    {
        Not
    }

    internal class UnitaryNode : ICouchNode
    {
        public UnitaryNodeType Type { get; set; }
        public ICouchNode Child { get; set; }
    }
}
