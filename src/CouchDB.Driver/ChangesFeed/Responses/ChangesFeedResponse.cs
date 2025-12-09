using System.Collections.Generic;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.ChangesFeed.Responses;

public class ChangesFeedResponse<TSource> where TSource : CouchDocument
{
    [JsonPropertyName("last_seq")]
    public string LastSequence { get; set; }

    [JsonPropertyName("pending")]
    public int Pending { get; set; }

    [JsonPropertyName("results")]
    public IList<ChangesFeedResponseResult<TSource>> Results { get; set; }
}