namespace CouchDB.Driver.DTOs;

[Serializable]
internal sealed record AttachmentResult(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("ok")]
    bool Ok,
    [property: JsonPropertyName("rev")]
    string Rev
);