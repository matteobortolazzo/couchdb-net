namespace CouchDB.Driver.ChangesFeed.Responses;

[Serializable]
public record ChangesFeedResponseResult<TSource>(
    [property: JsonPropertyName("seq")]
    string Seq,
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("deleted")]
    bool Deleted,
    [property: JsonPropertyName("changes")]
    ChangesFeedResponseResultChange[] Changes,
    [property: JsonPropertyName("roleIds")]
    string[]? RoleIds,
    [property: JsonPropertyName("createdAt")]
    DateTimeOffset? CreatedAt,
    [property: JsonPropertyName("createdBy")]
    string? CreatedBy,
    [property: JsonPropertyName("doc")]
    TSource? Document);