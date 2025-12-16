namespace CouchDB.Driver.DTOs;

[Serializable]
internal sealed record CouchError(
    [property: JsonPropertyName("error")]
    string? Error,
    [property: JsonPropertyName("reason")]
    string? Reason
);