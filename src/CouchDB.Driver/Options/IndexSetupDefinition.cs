using System;
using CouchDB.Driver.Indexes;
using CouchDB.Driver.Types;

namespace CouchDB.Driver.Options;

internal class IndexSetupDefinition<TSource>(
    string name,
    Action<IIndexBuilder<TSource>> indexBuilderAction,
    IndexOptions? options)
    where TSource : CouchDocument
{
    public string Name { get; } = name;
    public Action<IIndexBuilder<TSource>> IndexBuilderAction { get; } = indexBuilderAction;
    public IndexOptions? Options { get; } = options;
}