using CouchDB.Driver.Views;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal record RunViewQueriesResult<TKey, TValue, TSource>(
    [property: JsonPropertyName("results")]
    CouchViewList<TKey, TValue, TSource>[] Results)
    where TSource : class;