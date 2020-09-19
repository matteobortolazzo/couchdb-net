using System;
using System.Collections.Generic;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Options
{
    public class CouchDocumentBuilder<TSource>
        where TSource : CouchDocument
    {
        internal List<IndexSetupDefinition<TSource>> IndexDefinitions { get; }

        internal CouchDocumentBuilder()
        {
            IndexDefinitions = new List<IndexSetupDefinition<TSource>>();
        }

        public CouchDocumentBuilder<TSource> HasIndex(string name, Action<IIndexBuilder<TSource>> indexBuilderAction,
            IndexOptions? options = null)
        {
            var indexDefinition = new IndexSetupDefinition<TSource>(name, indexBuilderAction, options);
            IndexDefinitions.Add(indexDefinition);
            return this;
        }
    }
}