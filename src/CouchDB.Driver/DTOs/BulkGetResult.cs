using CouchDB.Driver.Types;

namespace CouchDB.Driver.DTOs;

[Serializable]
internal class BulkGetResult<TSource> where TSource : CouchDocument
{
    [property:JsonPropertyName("results")]
    public required List<BulkGetResultDoc<TSource>> Results { get; init; }
}