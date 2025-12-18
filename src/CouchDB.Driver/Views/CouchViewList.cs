namespace CouchDB.Driver.Views;

/// <summary>
/// Result of a view query.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TValue">The type of the value.</typeparam>
/// <typeparam name="TDoc">The type of the document.</typeparam>
[Serializable]
public class CouchViewList<TKey, TValue, TDoc>
    where TDoc : class
{
    /// <summary>
    /// Number of documents in the database/view.
    /// </summary>
    [property:JsonPropertyName("total_rows")]
    public int TotalRows { get; init; }

    /// <summary>
    /// Offset where the document list started.
    /// </summary>
    [property:JsonPropertyName("offset")]
    public int Offset { get; init; }

    /// <summary>
    /// Array of view row objects. This result contains the document ID, value and the documents.
    /// </summary>
    [property:JsonPropertyName("rows")]
    public required List<CouchView<TKey, TValue, TDoc>> Rows { get; init; }
}