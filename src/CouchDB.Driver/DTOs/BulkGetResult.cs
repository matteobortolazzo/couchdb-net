
namespace CouchDB.Driver.DTOs;

[Serializable]
internal class BulkGetResult<TSource> where TSource: class
{
    [property:JsonPropertyName("results")]
    public required List<BulkGetResultDoc<TSource>> Results { get; init; }
}