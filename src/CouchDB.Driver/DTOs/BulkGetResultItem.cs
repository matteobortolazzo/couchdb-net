
namespace CouchDB.Driver.DTOs;

[Serializable]
internal class BulkGetResultItem<TSource> where TSource: class
{
    [property:JsonPropertyName("ok")]
    public TSource? Item { get; init; }
}