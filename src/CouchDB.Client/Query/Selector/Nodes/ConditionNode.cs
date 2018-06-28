namespace CouchDB.Client.Query.Selector.Nodes
{
    internal enum ConditionNodeType
    {
        Equal, NotEqual, GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
    }

    internal class ConditionNode : ICouchNode
    {
        public ConditionNodeType Type { get; set; }
        public MemberNode PropertyNode { get; set; }
        public ConstantNode ValueNode { get; set; }
    }
}
