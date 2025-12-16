
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.ChangesFeed.Responses;

[Serializable]
public class ChangesFeedResponse<TSource> where TSource : CouchDocument
{
    [property:JsonPropertyName("last_seq")]
    public required string LastSequence { get; init; }

    [property:JsonPropertyName("pending")]
    public int Pending { get; init; }

    [property:JsonPropertyName("results")]
    public required IList<ChangesFeedResponseResult<TSource>> Results { get; set; }
}