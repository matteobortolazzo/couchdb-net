namespace CouchDB.Driver.DTOs;

[Serializable]
public record CreateAttachmentRequest(
    [property: JsonPropertyName("content_type")]
    string ContentType,
    [property: JsonPropertyName("data")]
    string Data);