using CouchDB.Driver.Types;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class BulkGetResultItem<TSource> where TSource : CouchDocument
{
    [property:JsonPropertyName("ok")]
    public TSource? Item { get; init; }
}