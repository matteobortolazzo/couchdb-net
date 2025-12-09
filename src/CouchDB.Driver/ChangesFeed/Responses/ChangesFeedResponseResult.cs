using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CouchDB.Driver.ChangesFeed.Responses;

[Serializable]
public class ChangesFeedResponseResult<TSource>
{
    [JsonPropertyName("seq")]
    public string Seq { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("deleted")]
    public bool Deleted { get; set; }

    [JsonPropertyName("changes")]
    public IList<ChangesFeedResponseResultChange> Changes { get; set; }

    [JsonPropertyName("roleIds")]
    public IList<string> RoleIds { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("createdBy")]
    public string CreatedBy { get; set; }

    [JsonPropertyName("doc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TSource Document { get; set; }
}