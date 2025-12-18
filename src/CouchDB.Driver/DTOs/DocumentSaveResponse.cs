namespace CouchDB.Driver.DTOs;

[Serializable]
internal sealed record DocumentSaveResponse(
    [property: JsonPropertyName("ok")]
    bool Ok,
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("rev")]
    string? Rev,
    [property: JsonPropertyName("error")]
    string? Error,
    [property: JsonPropertyName("reason")]
    string? Reason
);