using CouchDB.Driver.Types;
using CouchDB.Driver.Views;

namespace CouchDB.Driver.DTOs;

// TODO: Review
[Serializable]
internal class CouchViewQueryResult<TKey, TValue, TDoc>
    where TDoc : CouchDocument
{
    /// <summary>
    /// The results in the same order as the queries.
    /// </summary>
    [property:JsonPropertyName("results")]
    public required CouchViewList<TKey, TValue, TDoc>[] Results { get; init; }
}