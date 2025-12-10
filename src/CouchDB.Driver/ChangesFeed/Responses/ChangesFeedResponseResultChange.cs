using System;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.ChangesFeed.Responses;

[Serializable]
public class ChangesFeedResponseResultChange
{
    [JsonPropertyName("rev")]
    public required string Rev { get; init; }
}