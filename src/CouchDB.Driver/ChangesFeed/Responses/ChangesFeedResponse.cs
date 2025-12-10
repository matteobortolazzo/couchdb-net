using System;
using System.Collections.Generic;
using CouchDB.Driver.Types;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.ChangesFeed.Responses;

[Serializable]
public class ChangesFeedResponse<TSource> where TSource : CouchDocument
{
    [JsonPropertyName("last_seq")]
    public required string LastSequence { get; init; }

    [JsonPropertyName("pending")]
    public int Pending { get; init; }

    [JsonPropertyName("results")]
    public required IList<ChangesFeedResponseResult<TSource>> Results { get; set; }
}