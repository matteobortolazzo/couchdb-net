using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.ChangesFeed.Responses;

[Serializable]
public class ChangesFeedResponseResult<TSource>
{
    [JsonPropertyName("seq")]
    public required string Seq { get; init; }

    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("deleted")]
    public required bool Deleted { get; init; }

    [JsonPropertyName("changes")]
    public required IList<ChangesFeedResponseResultChange> Changes { get; init; }

    [JsonPropertyName("roleIds")]
    public required IList<string> RoleIds { get; init; }

    [JsonPropertyName("createdAt")]
    public required DateTime CreatedAt { get; init; }

    [JsonPropertyName("createdBy")]
    public required string CreatedBy { get; init; }

    [JsonPropertyName("doc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required TSource Document { get; set; }
}