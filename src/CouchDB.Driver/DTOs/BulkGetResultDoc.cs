using CouchDB.Driver.Types;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class BulkGetResultDoc<TSource> where TSource : CouchDocument
{
    [property:JsonPropertyName("docs")]
    public required List<BulkGetResultItem<TSource>> Docs { get; init; }
}