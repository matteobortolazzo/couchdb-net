
namespace CouchDB.Driver.DTOs;

[Serializable]
internal class BulkGetResultDoc<TSource> where TSource: class
{
    [property:JsonPropertyName("docs")]
    public required List<BulkGetResultItem<TSource>> Docs { get; init; }
}