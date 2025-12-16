namespace CouchDB.Driver.DTOs;

[Serializable]
internal class AttachmentResult
{
    [property:JsonPropertyName("id")]
    public required string Id { get; init; }

    [property:JsonPropertyName("ok")]
    public bool Ok { get; init; }

    [property:JsonPropertyName("rev")]
    public required string Rev { get; init; }
}