using System;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Options
{
    internal class IndexDefinition<TSource>
        where TSource : CouchDocument
    {
        public IndexDefinition(string name, Action<IIndexBuilder<TSource>> indexBuilderAction, IndexOptions? options)
        {
            Name = name;
            IndexBuilderAction = indexBuilderAction;
            Options = options;
        }

        public string Name { get; }
        public Action<IIndexBuilder<TSource>> IndexBuilderAction { get; }
        public IndexOptions? Options { get; }
    }
}