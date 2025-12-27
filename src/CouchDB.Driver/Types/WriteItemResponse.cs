namespace CouchDB.Driver.Types;

/// <summary>
/// Represents the response received after a document request.
/// </summary>
/// <param name="Id">Document ID</param>
/// <param name="Rev">Document revision</param>
[Serializable]
public sealed record WriteItemResponse(
    [property: JsonPropertyName("id")]
    string Id,
    [property: JsonPropertyName("rev")]
    string Rev
);

/// <summary>
/// Represents the response received after a bulk document request.
/// </summary>
/// <param name="Id">Document ID</param>
/// <param name="Rev">Document revision on success</param>
/// <param name="Error">Error type</param>
/// <param name="Reason">Error reason</param>
[Serializable]
public sealed record BulkWriteItemResponse(
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
