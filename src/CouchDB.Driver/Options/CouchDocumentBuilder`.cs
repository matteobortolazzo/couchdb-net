using CouchDB.Driver.Indexes;
using CouchDB.Driver.Types;
using System;
using System.Collections.Generic;

namespace CouchDB.Driver.Options
{
    public class CouchDocumentBuilder<TSource> : CouchDocumentBuilder
        where TSource : CouchDocument
    {
        internal List<IndexSetupDefinition<TSource>> IndexDefinitions { get; }

        internal CouchDocumentBuilder()
        {
            IndexDefinitions = new List<IndexSetupDefinition<TSource>>();
        }

        public CouchDocumentBuilder<TSource> ToDatabase(string database)
        {
            Database = database;
            return this;
        }

        public CouchDocumentBuilder<TSource> WithShards(int shards)
        {
            Shards = shards;
            return this;
        }

        public CouchDocumentBuilder<TSource> WithReplicas(int replicas)
        {
            Replicas = replicas;
            return this;
        }

        public CouchDocumentBuilder<TSource> IsPartitioned()
        {
            Partitioned = true;
            return this;
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
