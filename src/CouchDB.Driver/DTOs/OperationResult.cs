namespace CouchDB.Driver.DTOs;

[Serializable]
internal sealed record OperationResult(
    [property: JsonPropertyName("ok")]
    bool Ok
);