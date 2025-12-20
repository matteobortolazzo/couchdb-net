namespace CouchDB.Driver.ChangesFeed.Responses;

[Serializable]
public class ChangesFeedResponseResult<TSource>
{
    [property:JsonPropertyName("seq")]
    public required string Seq { get; init; }

    [property:JsonPropertyName("id")]
    public required string Id { get; init; }

    [property:JsonPropertyName("deleted")]
    public bool Deleted { get; init; }

    [property:JsonPropertyName("changes")]
    public required IList<ChangesFeedResponseResultChange> Changes { get; init; }

    [property:JsonPropertyName("roleIds")]
    public IList<string>? RoleIds { get; init; }

    [property:JsonPropertyName("createdAt")]
    public DateTimeOffset? CreatedAt { get; init; }

    [property:JsonPropertyName("createdBy")]
    public string? CreatedBy { get; init; }

    [property:JsonPropertyName("doc")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TSource? Document { get; init; }
}