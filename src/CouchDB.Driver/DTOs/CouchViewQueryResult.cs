using CouchDB.Driver.Views;

namespace CouchDB.Driver.DTOs;

// TODO: Review
[Serializable]
internal class CouchViewQueryResult<TKey, TValue, TSource>
    where TSource : class
{
    /// <summary>
    /// The results in the same order as the queries.
    /// </summary>
    [property:JsonPropertyName("results")]
    public required CouchViewList<TKey, TValue, TSource>[] Results { get; init; }
}