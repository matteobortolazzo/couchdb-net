using System;
using System.Collections.Generic;
using System.Text;

namespace CouchDB.Client.Query.Sort
{
    internal class SortProperty
    {
        public string Name { get; }
        public string Direction { get; }

        public SortProperty(string name, bool ascending)
        {
            Name = name;
            Direction = ascending ? "asc" : "desc";
        }
    }
}
