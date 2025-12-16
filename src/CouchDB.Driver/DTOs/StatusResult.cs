namespace CouchDB.Driver.DTOs;

[Serializable]
internal sealed record StatusResult(
    [property: JsonPropertyName("status")]
    string Status
);