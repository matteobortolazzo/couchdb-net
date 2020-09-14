using System;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Options
{
    public class CouchDocumentBuilder<TSource>
        where TSource : CouchDocument
    {
        internal string Name { get; set; }
        internal Action<IIndexBuilder<TSource>> IndexBuilderAction { get; set; }
        internal IndexOptions? Options { get; set; }

        internal CouchDocumentBuilder()
        {
            Name = string.Empty;
            IndexBuilderAction = builder => {};
        }

        public CouchDocumentBuilder<TSource> HasIndex(string name, Action<IIndexBuilder<TSource>> indexBuilderAction,
            IndexOptions? options = null)
        {
            Name = name;
            IndexBuilderAction = indexBuilderAction;
            Options = options;
            return this;
        }
    }
}