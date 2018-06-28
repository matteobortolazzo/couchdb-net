namespace CouchDB.Client.Query.Selector.Nodes
{
    internal class ConstantNode : ICouchNode
    {
        public object Value { get; set; }
    }
}
