using CouchDB.Driver.Converters;

namespace CouchDB.Driver.Views;

/// <summary>
/// Result of a view query.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TDoc">The type of the document.</typeparam>
[Serializable]
[JsonConverter(typeof(CouchViewListConverterFactory))]
public class CouchViewList<TKey, TValue, TDoc>(
    IReadOnlyList<CouchView<TKey, TValue, TDoc>> source,
    int totalRows,
    int offset)
    : IReadOnlyList<CouchView<TKey, TValue, TDoc>>
    where TDoc : class
{
    /// <summary>
    /// Number of documents in the database/view.
    /// </summary>
    public int TotalRows { get; } = totalRows;

    /// <summary>
    /// Offset where the document list started.
    /// </summary>
    public int Offset { get; } = offset;

    public int Count => source.Count;
    public CouchView<TKey, TValue, TDoc> this[int index] => source[index];

    public IEnumerator<CouchView<TKey, TValue, TDoc>> GetEnumerator()
    {
        return source.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return source.GetEnumerator();
    }
}