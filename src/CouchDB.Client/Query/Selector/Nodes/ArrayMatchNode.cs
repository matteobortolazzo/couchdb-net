using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Client.Query.Selector.Nodes
{
    internal enum ArrayMatchNodeType
    {
        All, Any
    }

    internal class ArrayMatchNode : ICouchNode
    {
        public ArrayMatchNodeType Type { get; set; }
        public MemberNode PropertyNode { get; set; }
        public ICouchNode ArraySelectorNode { get; set; }
    }
}
