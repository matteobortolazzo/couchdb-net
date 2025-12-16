using System.Collections;
using System.Collections.Generic;

namespace CouchDB.Driver.Types;

/// <summary>
/// Represents a Couch query response.
/// </summary>
/// <typeparam name="TSource"></typeparam>
public class CouchList<TSource>(
    IReadOnlyList<TSource> source,
    string bookmark,
    ExecutionStats? executionStats)
    : IReadOnlyList<TSource>
{
    /// <summary>
    /// An opaque string used for paging.
    /// </summary>
    public string Bookmark { get; } = bookmark;

    /// <summary>
    /// Execution statistics.
    /// </summary>
    public ExecutionStats? ExecutionStats { get; } = executionStats;

    public int Count => source.Count;
    public bool IsReadOnly => true;
    public TSource this[int index] => source[index];

    public IEnumerator<TSource> GetEnumerator()
    {
        return source.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return source.GetEnumerator();
    }
}