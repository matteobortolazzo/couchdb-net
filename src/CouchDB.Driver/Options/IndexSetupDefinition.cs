using System;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Options
{
    internal class IndexSetupDefinition<TSource>
        where TSource : CouchDocument
    {
        public IndexSetupDefinition(string name, Action<IIndexBuilder<TSource>> indexBuilderAction, IndexOptions? options)
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