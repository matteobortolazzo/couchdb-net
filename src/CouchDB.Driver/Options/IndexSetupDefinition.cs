using System;
using CouchDB.Driver.Indexes;

namespace CouchDB.Driver.Options;

internal class IndexSetupDefinition<TSource>(
    string name,
    Action<IIndexBuilder<TSource>> indexBuilderAction,
    IndexOptions? options)
    where TSource: class
{
    public string Name { get; } = name;
    public Action<IIndexBuilder<TSource>> IndexBuilderAction { get; } = indexBuilderAction;
    public IndexOptions? Options { get; } = options;
}